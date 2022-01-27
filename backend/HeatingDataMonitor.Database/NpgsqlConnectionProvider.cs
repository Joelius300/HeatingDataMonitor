using Microsoft.Extensions.Logging;
using Npgsql;

namespace HeatingDataMonitor.Database;

internal class NpgsqlConnectionProvider : IConnectionProvider<NpgsqlConnection>
{
    private readonly string _connectionString;
    private readonly ILogger<NpgsqlConnectionProvider> _logger;

    public NpgsqlConnectionProvider(string connectionString, ILogger<NpgsqlConnectionProvider> logger)
    {
        _connectionString = connectionString;
        _logger = logger;

        if (!connectionString.Contains("Max Auto Prepare"))
        {
            // https://www.npgsql.org/doc/prepare.html#automatic-preparation
            _logger.LogWarning(
                "Connection string doesn't contain 'Max Auto Prepare', which means Dapper won't take advantage of prepared statements; See source code for more info");
        }
    }

    public async Task<NpgsqlConnection> OpenConnection()
    {
        NpgsqlConnection? connection = null;

        try
        {
            connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            // enables the use of nodatimes Instant and LocalDateTime in models on Npgsqls side.
            connection.TypeMapper.UseNodaTime(); // connection has to be open already

            return connection;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't open postgres connection");
            if (connection is not null)
                await connection.DisposeAsync();

            throw;
        }
    }
}
