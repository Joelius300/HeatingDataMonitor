using Dapper;
using HeatingDataMonitor.Database;
using NodaTime;
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
            // can't execute commands on this connection as it starts waiting again instantly
            // because of the while so we open a new one (it's pooled anyway).
            await using var innerConnection = await _connectionProvider.OpenConnection();
            Instant time =
                innerConnection.ExecuteScalar<Instant>(
                    "SELECT received_time from heating_data order by received_time DESC LIMIT 1;");
            Console.WriteLine($"Received event: {time}");
        };

        await connection.ExecuteAsync("LISTEN record_added;");

        cancellationToken.Register(() => Console.WriteLine("Token was for sure cancelled"));
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // apparently WaitAsync's cancellation is fucked?
                // apparently also it's timeout is fucked? I don't understand, it seems stuck as soon as I Ctrl+C.
                if (!await connection.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
                {
                    Console.WriteLine("Timeout?");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Wait async cancelled");
            }
        }

        await connection.ExecuteAsync("UNLISTEN record_added;");

        Console.WriteLine("DoStuff ended");
    }
}
