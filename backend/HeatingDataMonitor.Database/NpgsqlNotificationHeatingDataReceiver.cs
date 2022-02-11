using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Dapper;
using HeatingDataMonitor.Models;
using HeatingDataMonitor.Receiver.Shared;
using Npgsql;

namespace HeatingDataMonitor.Database;

// WIP! Reconsider this carefully but I think it might actually be nicer / easier to do it the same way as the Serial
// port stuff because implementing IAsyncEnumerator instead of anything higher gives full control over cleanup with
// IAsyncDisposable and allows use of class members instead of lots of local variables.
public class NpgsqlNotificationHeatingDataReceiver : IHeatingDataReceiver
{
    private const string ChannelName = "record_added"; // could be made more configurable, doesn't need to be tho
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;

    public NpgsqlNotificationHeatingDataReceiver(IConnectionProvider<NpgsqlConnection> connectionProvider) => _connectionProvider = connectionProvider;

    public async IAsyncEnumerable<HeatingData> StreamHeatingData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _connectionProvider.OpenConnection();
        // Use a queue with a max capacity of 100 to prevent infinite memory growth. Limit shouldn't ever be hit -> exception if it does happen
        Channel<HeatingData> queue = Channel.CreateBounded<HeatingData>(new BoundedChannelOptions(100) {SingleReader = true, SingleWriter = true});
        // ReSharper disable once AccessToDisposedClosure
        await using CancellationTokenRegistration unregisterCallback = cancellationToken.Register(() =>
        {
            connection.Dispose(); // this makes WaitAsync throw an OperationCanceledException, which will terminate the waiting loop
            queue.Writer.Complete(); // stop accepting new
        });
        connection.Notification += AddRecordToQueue;
        await connection.ExecuteAsync($"LISTEN {ChannelName};");

        // Not 100% sure if that's the right way to do this. Since Tasks are hot in .NET, I could also just call the
        // method without awaiting. IIRC this just enables .NET to move it to a different thread if it wants to.
        // ReSharper disable once AccessToDisposedClosure
        using Task notificationLoop =
            Task.Run(() => WaitForConnections(connection, cancellationToken), cancellationToken);

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

    private static async Task WaitForConnections(NpgsqlConnection connection, CancellationToken cancellationToken)
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
