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

// TODO VERY IMPORTANT Test if this works for long durations. If not we may need keepalives.
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
        private volatile bool _disposed;

        public HeatingData Current { get; private set; } = null!;

        public Enumerator(IConnectionProvider<NpgsqlConnection> connectionProvider, ILogger<NpgsqlNotificationHeatingDataReceiver> logger, CancellationToken cancellationToken)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
            _cancellationToken = cancellationToken;

            // Use a queue with a max capacity of 100 to prevent infinite memory growth. Limit shouldn't ever be hit -> log error and stop operation if it does happen
            _queue = Channel.CreateBounded<HeatingData>(new BoundedChannelOptions(100) {SingleReader = true, SingleWriter = true});
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            // one could check for disposed and throw ObjectDisposedException but that would also
            // disallow reading the 100 items in the queue after it hit the capacity limit.
            // Either works and since the issue that caused the queue to overflow is most likely
            // a caller that blocks inside an await foreach loop (MoveNextAsync not called for a long time)
            // it doesn't really matter since this method won't be called anyway and an error has already been logged.
            // if anything changes in that regard, it should be in sync with the serial port enumerator.

            if (_cancellationToken.IsCancellationRequested)
                return false;

            if (!_hasBeenSetup)
            {
                _hasBeenSetup = true; // before setup so that dispose is still called if something fails during setup
                await SetupConnection();
            }

            bool hasMoreData;
            try
            {
                hasMoreData = await _queue.Reader.WaitToReadAsync(_cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // stop enumeration, handled below
                hasMoreData = false;
            }
            catch when (_queue.Reader.Completion.IsCompleted)
            {
                // This catch block catches all exceptions that happen due to the notification loop throwing
                // an exception (that isn't a cancellation) in which case the queue will be completed with an error.
                // That error is thrown from the WaitToReadAsync call and caught here to set the loop task to null
                // and avoid unnecessary further handling of it during disposal before rethrowing the original exception.
                _notificationLoopTask = null;

                throw;
            }

            if (!hasMoreData)
            {
                // not 100% sure but this assert might fail when the queue is full because then disposal happens
                // before cancellation (which might never happen). hasMoreData would still be false eventually
                // because after disposal no more data will be written to the queue.
                // One could throw an ObjectDisposedException here (as done with the serial port data reader) but
                // because I'm not sure about the specifics of the situation (and small differences with the serial port
                // case) I'll just end the enumeration.
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

                if (!_queue.Writer.TryWrite(record) && !_disposed) // writing to the queue obviously fails after disposal
                {
                    _logger.LogError("Couldn't write to the queue (probably because it's full which " +
                                     "could indicate blocking inside loop body or a critical issue somewhere else)");
                    await DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                // exception from this method couldn't be caught outside (event and async void).
                _logger.LogError(ex, "Couldn't fetch and enqueue newly inserted record from db");
            }
        }

        private async Task WaitForNotifications()
        {
            // passing the cancellation token to StreamHeatingData or WithCancellation is easy to handle
            // but this waiting task also has to end when break is used. In that case DisposeAsync is called
            // without cancellation so we set _disposed to true, which will gracefully terminate this loop (after max ~500ms).
            while (!_cancellationToken.IsCancellationRequested && !_disposed)
            {
                try
                {
                    // let cancellation exceptions bubble up (be bound to the task) as they will put the
                    // task in Canceled state which we can ignore.
                    // Everything else is caught and used to complete the queue before being rethrown.
                    await _connection!.WaitAsync(500, _cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // causes the caught exception to be thrown in the correct context (MoveNextAsync)
                    _queue.Writer.Complete(ex);

                    throw; // puts the task in Faulted state
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed || !_hasBeenSetup)
                return;

            _disposed = true;

            _queue.Writer.TryComplete();

            if (_notificationLoopTask is not null &&
                !_notificationLoopTask.IsCompletedSuccessfully &&
                !_notificationLoopTask.IsCanceled)
            {
                // - for a successful or canceled task we don't do anything
                // - for an unfinished task we await until it terminates, for a maximum of 1 second
                //   before we move on to dispose the connection
                // - the task shouldn't be faulted because if a non-cancellation exception occured, it was rethrown in
                //   the correct context and then the task was set to null. however, if it somehow still happens, await
                //   ensures that the exception will be thrown at least now.
                await Task.WhenAny(_notificationLoopTask, Task.Delay(1000));
                if (!_notificationLoopTask.IsCompletedSuccessfully)
                {
                    _logger.LogWarning("Notification loop didn't finish within 1 second during disposal");
                }
                else
                {
                    _logger.LogDebug("Notification loop finished gracefully");
                }
            }

            if (_connection is not null)
            {
                try
                {
                    // this can easily fail if e.g. the network cut out or something like that
                    await _connection.ExecuteAsync($"UNLISTEN {NotificationChannelName};");
                }
                catch (NpgsqlException ex)
                {
                    _logger.LogWarning(ex, "Error while trying to unlisten from notification channel");
                }

                _connection.Notification -= AddRecordToQueue;
                // if the connection is still waiting, this will throw an exception; that's why we wait for the loop task beforehand
                await _connection.DisposeAsync();
                _connection = null;
                _logger.LogDebug("Connection disposed successfully");
            }
        }
    }
}
