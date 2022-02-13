using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Dapper;
using HeatingDataMonitor.Models;
using HeatingDataMonitor.Receiver.Shared;
using Npgsql;

namespace HeatingDataMonitor.Database;

// TODO integration tests for this class would make sense. You could use .NET testcontainers.
public class NpgsqlNotificationHeatingDataReceiver : IHeatingDataReceiver
{
    public const string ChannelName = "record_added"; // could be made more configurable, doesn't really need to be
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;

    public NpgsqlNotificationHeatingDataReceiver(IConnectionProvider<NpgsqlConnection> connectionProvider) => _connectionProvider = connectionProvider;

    public IAsyncEnumerable<HeatingData> StreamHeatingData(CancellationToken cancellationToken) =>
        new Enumerable(cancellationToken);

    private class Enumerable : IAsyncEnumerable<HeatingData>
    {
        private readonly CancellationToken _defaultCancellationToken; // may be overridden by WithCancellation
        private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;
        private readonly Channel<HeatingData> _queue;
        private NpgsqlConnection? _connection; // creating a connection is async so we can't do it in the constructor
        private Task _notificationLoopTask;

        public Enumerable(CancellationToken defaultCancellationToken, IConnectionProvider<NpgsqlConnection> connectionProvider)
        {
            _defaultCancellationToken = defaultCancellationToken;
            _connectionProvider = connectionProvider;

            // Use a queue with a max capacity of 100 to prevent infinite memory growth. Limit shouldn't ever be hit -> exception if it does happen
            _queue = Channel.CreateBounded<HeatingData>(new BoundedChannelOptions(100) {SingleReader = true, SingleWriter = true});
        }

        // TODO This is all good and splitting into a setup and cleanup function is great BUT the enumerable isn't
        // disposed (think of List, why would the list be disposed after you're done iterating over it?) so it would make
        // more sense to have the receiver implement IAsyncEnumerable (just like the serial port one) and (although that's
        // slightly less nice than yield return etc) manually implement the enumerator which gives full control over cleanup via IAsyncDisposal.
        // That probably also means you're going to need the two step thing for cancellation which you still have in your stash :)
        public async IAsyncEnumerator<HeatingData> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // parameter here (from WithCancellation) has priority over parameter in constructor (from StreamHeatingData)
            if (cancellationToken == CancellationToken.None)
            {
                cancellationToken = _defaultCancellationToken;
            }

            await SetupConnection(cancellationToken);

            // TODO implement this logic in an enumerator (setup once in movenext with guard, cleanup in disposeasync).
            // Here you'll only need together with the default token logic to also allow WithCancellation.
            return new Enumerator(cancellationToken);
        }

        private async Task SetupConnection(CancellationToken cancellationToken)
        {
            _connection = await _connectionProvider.OpenConnection();
            _connection.Notification += AddRecordToQueue;
            _notificationLoopTask = Task.Run(() => WaitForNotifications(_connection, cancellationToken), cancellationToken);
        }
    }

    public async IAsyncEnumerable<HeatingData> StreamHeatingData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _connectionProvider.OpenConnection();
        // ReSharper disable once AccessToDisposedClosure
        await using CancellationTokenRegistration unregisterCallback = cancellationToken.Register(() =>
        {
            connection.Dispose(); // this makes WaitAsync throw an OperationCanceledException, which will terminate the waiting loop
            queue.Writer.Complete(); // stop accepting new
        });
        await connection.ExecuteAsync($"LISTEN {ChannelName};");

        // Not 100% sure if that's the right way to do this. Since Tasks are hot in .NET, I could also just call the
        // method without awaiting. IIRC this just enables .NET to move it to a different thread if it wants to.
        // ReSharper disable once AccessToDisposedClosure
        using Task

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool hasMoreData = false;
                try
                {
                    hasMoreData = await queue.Reader.WaitToReadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // stop enumeration, handled below
                }

                if (!hasMoreData)
                {
                    Debug.Assert(cancellationToken.IsCancellationRequested,
                        "No more data without cancellation being requested");
                    break;
                }

                bool couldRead = queue.Reader.TryRead(out HeatingData? record);
                Debug.Assert(couldRead && record is not null, "couldRead && record is not null");

                yield return record!;
            }
        }
        finally
        {
            connection.Notification -= AddRecordToQueue;
            await notificationLoop; // let the loop finish and bubble up exceptions if there were any
        }
    }

    private void AddRecordToQueue(object sender, NpgsqlNotificationEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static async Task WaitForNotifications(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await connection.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!cancellationToken.IsCancellationRequested)
                    throw; // if the cancellation wasn't requested, there might be an underlying issue.
            }
        }
    }
}
