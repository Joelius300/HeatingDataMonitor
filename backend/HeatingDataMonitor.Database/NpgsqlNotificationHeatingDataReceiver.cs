using System.Diagnostics;
using System.Threading.Channels;
using Dapper;
using HeatingDataMonitor.Models;
using HeatingDataMonitor.Receiver.Shared;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using Npgsql;

// ReSharper disable ContextualLoggerProblem

namespace HeatingDataMonitor.Database;

// TODO integration tests for this class would make sense. You could use .NET testcontainers.
// TODO Think through what SQL makes sense to be in here and what SQL should be put into the read repo.
internal class NpgsqlNotificationHeatingDataReceiver : IHeatingDataReceiver
{
    private const string NotificationChannelName = "record_added"; // could be made more configurable, doesn't really need to be
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;
    private readonly ILogger<NpgsqlNotificationHeatingDataReceiver> _logger;

    public NpgsqlNotificationHeatingDataReceiver(IConnectionProvider<NpgsqlConnection> connectionProvider, ILogger<NpgsqlNotificationHeatingDataReceiver> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public IAsyncEnumerable<HeatingData> StreamHeatingData(CancellationToken cancellationToken) =>
        new Enumerable(_connectionProvider, _logger, cancellationToken);

    private class Enumerable : IAsyncEnumerable<HeatingData>
    {
        private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;
        private readonly ILogger<NpgsqlNotificationHeatingDataReceiver> _logger;
        private readonly CancellationToken _defaultCancellationToken; // may be overridden by WithCancellation

        public Enumerable(IConnectionProvider<NpgsqlConnection> connectionProvider, ILogger<NpgsqlNotificationHeatingDataReceiver> logger, CancellationToken defaultCancellationToken)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
            _defaultCancellationToken = defaultCancellationToken;
        }

        public IAsyncEnumerator<HeatingData> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // parameter here (from WithCancellation) has priority over parameter in constructor (from StreamHeatingData)
            if (cancellationToken == CancellationToken.None)
            {
                cancellationToken = _defaultCancellationToken;
            }

            return new Enumerator(_connectionProvider, _logger, cancellationToken);
        }
    }

    private class Enumerator : IAsyncEnumerator<HeatingData>
    {
        private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly ILogger<NpgsqlNotificationHeatingDataReceiver> _logger;
        private readonly Channel<HeatingData> _queue;
        private NpgsqlConnection? _connection; // creating a connection is async so we can't do it in the constructor
        private Task? _notificationLoopTask;
        private bool _hasBeenSetup;
        private bool _disposed;

        public HeatingData Current { get; private set; } = null!;

        public Enumerator(IConnectionProvider<NpgsqlConnection> connectionProvider, ILogger<NpgsqlNotificationHeatingDataReceiver> logger, CancellationToken cancellationToken)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
            _cancellationToken = cancellationToken;

            // Use a queue with a max capacity of 100 to prevent infinite memory growth. Limit shouldn't ever be hit -> exception if it does happen
            _queue = Channel.CreateBounded<HeatingData>(new BoundedChannelOptions(100) {SingleReader = true, SingleWriter = true});
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_cancellationToken.IsCancellationRequested)
                return false;

            if (!_hasBeenSetup)
            {
                _hasBeenSetup = true; // before setup so that dispose is still called if something fails during setup
                await SetupConnection();
            }

            bool hasMoreData = false;
            try
            {
                hasMoreData = await _queue.Reader.WaitToReadAsync(_cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // stop enumeration, handled below
            }

            if (!hasMoreData)
            {
                Debug.Assert(_cancellationToken.IsCancellationRequested,
                    "No more data without cancellation being requested");

                return false;
            }


            bool couldRead = _queue.Reader.TryRead(out HeatingData? record);
            Debug.Assert(couldRead && record is not null, "couldRead && record is not null; after waiting for queue");

            Current = record;

            return true;
        }

        private async Task SetupConnection()
        {
            _connection = await _connectionProvider.OpenConnection();
            _connection.Notification += AddRecordToQueue;
            await _connection.ExecuteAsync($"LISTEN {NotificationChannelName};");
            _notificationLoopTask = Task.Run(WaitForNotifications, _cancellationToken);
        }

        private async void AddRecordToQueue(object sender, NpgsqlNotificationEventArgs e)
        {
            try
            {
                Instant time = InstantPattern.ExtendedIso.Parse(e.Payload).GetValueOrThrow();

                await using NpgsqlConnection fetchingConnection = await _connectionProvider.OpenConnection();
                HeatingData record = await fetchingConnection.QuerySingleAsync<HeatingData>(
                    "SELECT * from heating_data where received_time = @time LIMIT 1;", new {time});

                _queue.Writer.TryWrite(record); // we don't care if it succeeds because if it didn't we'd just want to return
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't fetch and enqueue newly inserted record from db");
            }
        }

        private async Task WaitForNotifications()
        {
            while (!_cancellationToken.IsCancellationRequested && _connection is not null)
            {
                // let cancellation exceptions bubble up as they will put the task in Canceled state which we can ignore.
                await _connection.WaitAsync(_cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed || !_hasBeenSetup)
                return;

            _disposed = true;

            _queue.Writer.Complete();

            if (_connection is not null)
            {
                await _connection.ExecuteAsync($"UNLISTEN {NotificationChannelName};");
                _connection.Notification -= AddRecordToQueue;
                await _connection.DisposeAsync();
                _connection = null;
            }

            if (_notificationLoopTask is not null &&
                !_notificationLoopTask.IsCompletedSuccessfully &&
                !_notificationLoopTask.IsCanceled)
            {
                // for successful and canceled tasks we don't do anything
                // for faulted tasks we await to throw the exception
                // for unfinished tasks we await until they terminate
                await _notificationLoopTask;
            }
        }
    }
}
