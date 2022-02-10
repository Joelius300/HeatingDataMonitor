using Dapper;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Models;
using NodaTime;
using NodaTime.Text;
using Npgsql;

namespace TestingGrounds;

public class PostgresNotificationStuff
{
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;

    public PostgresNotificationStuff(IConnectionProvider<NpgsqlConnection> connectionProvider) => _connectionProvider = connectionProvider;

    public async Task DoStuff(CancellationToken cancellationToken)
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

        cancellationToken.Register(async () =>
        {
            Console.WriteLine("Token was for sure cancelled; disposing connection");
            await connection.DisposeAsync();
        });

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // apparently WaitAsync's cancellation is fucked?
                // apparently also it's timeout is fucked? I don't understand, it seems stuck as soon as I Ctrl+C.
                // SOLUTION: Manually dispose connection when cancellationToken is cancelled, that makes WaitAsync throw an OperationCanceledException.
                await connection.WaitAsync(cancellationToken);
                /*
                if (!await connection.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
                {
                    Console.WriteLine("Timeout?");
                }
                */
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Wait async cancelled");
            }
        }

        await using var unlistenConnection = await _connectionProvider.OpenConnection();
        await unlistenConnection.ExecuteAsync("UNLISTEN record_added;");

        Console.WriteLine("DoStuff ended");
    }
}
