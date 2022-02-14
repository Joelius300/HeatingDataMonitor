using Dapper;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Models;
using HeatingDataMonitor.Receiver.Shared;
using NodaTime;
using NodaTime.Text;
using Npgsql;

namespace TestingGrounds;

public class PostgresNotificationStuff
{
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider = null!;
    private readonly IHeatingDataReceiver _heatingDataReceiver;

    public PostgresNotificationStuff(IHeatingDataReceiver heatingDataReceiver)
    {
        _heatingDataReceiver = heatingDataReceiver;
    }

    public async Task DoStuff(CancellationToken cancellationToken)
    {
        await foreach (HeatingData record in _heatingDataReceiver.StreamHeatingData(cancellationToken))
        {
            Console.WriteLine($"Received record: {InstantPattern.ExtendedIso.Format(record.ReceivedTime)} - {record.SPS_Zeit}");
        }
    }

    public async Task DoStuffBeforeWeImplementedIt(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.OpenConnection();

        connection.Notification += async (sender, args) =>
        {
            string timeStr = args.Payload;
            Instant time = InstantPattern.ExtendedIso.Parse(timeStr).Value;
            Console.WriteLine($"Event payload: {timeStr} -> {InstantPattern.ExtendedIso.Format(time)}");
            // can't execute commands on this connection as it starts waiting again instantly
            // because of the while so we open a new one (it's pooled anyway).
            await using var innerConnection = await _connectionProvider.OpenConnection();
            HeatingData record =
                await innerConnection.QueryFirstAsync<HeatingData>(
                    "SELECT * from heating_data where received_time = @time LIMIT 1;", new {time});
            Console.WriteLine($"Received event: {InstantPattern.ExtendedIso.Format(record.ReceivedTime)} - {record.SPS_Zeit}");
        };

        await connection.ExecuteAsync("LISTEN record_added;");

        /*
        cancellationToken.Register(() =>
        {
            Console.WriteLine("Token was for sure cancelled; NOT disposing connection");

            //await connection.DisposeAsync();
        });
        */

        /*
        Task.Run(async () =>
        {
            await Task.Delay(10000);
            Console.WriteLine("10 seconds have passed, disposing connection, but listening again on a different one");
            connection.DisposeAsync();
            await using var unlistenConnection = await _connectionProvider.OpenConnection();
            unlistenConnection.Notification += (o, e) => Console.WriteLine("Got notification on new connection.");
            await Task.Delay(100_000, cancellationToken);
*/

            /* This doesn't work, notifications are still received.
            Console.WriteLine("10 seconds have passed, calling unlisten on a new connection.");

            await using var unlistenConnection = await _connectionProvider.OpenConnection();
            await unlistenConnection.ExecuteAsync("UNLISTEN record_added;");
            */
        //});

        Console.WriteLine("before loop");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // apparently WaitAsync's cancellation is fucked?
                // apparently also it's timeout is fucked? I don't understand, it seems stuck as soon as I Ctrl+C.
                // SOLUTION: Manually dispose connection when cancellationToken is cancelled, that makes WaitAsync throw an OperationCanceledException.
                // but that's a shit solution cause then you can't unlisten because that has to happen on the same connector.. It will automatically stop all listens
                // once
                // await connection.WaitAsync(cancellationToken);

                Console.WriteLine("Starting to wait for connection");


                // if (!await connection.WaitAsync(TimeSpan.FromSeconds(2), CancellationToken.None)) THIS WORKS, just takes two seconds for cancellation to happen
                // if (!await connection.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken)) WORKS AS WELL
                    //Console.WriteLine("Timeout?");

                // Works and is best :) I have no clue how that cancellation handler with Ctrl+C fucked everything up that badly
                await connection.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Wait async cancelled");
            }

            Console.WriteLine($"Token cancelled? {cancellationToken.IsCancellationRequested}");
        }

        Console.WriteLine("reached end of the loop");

        await Task.Delay(1000);

        await connection.ExecuteAsync("UNLISTEN record_added;");

        Console.WriteLine("DoStuff ended");
    }
}
