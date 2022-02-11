using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using NodaTime;
using Npgsql;

namespace HeatingDataMonitor.Database;

internal class NpgsqlConnectionProvider : IConnectionProvider<NpgsqlConnection>
{
    private readonly string _connectionString;
    private readonly ILogger<NpgsqlConnectionProvider> _logger;

    static NpgsqlConnectionProvider()
    {
        // I would love to scope this to only one repo or only a certain connection
        // or whatever but that's a lot of work for something entirely useless in this project
        // as we only have one database with one model and that will probably stay like that forever.
        // https://stackoverflow.com/questions/14814972/dapper-map-to-sql-column-with-spaces-in-column-names
        // Allow snake_case -> PascalCase mapping in Dapper
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        // Allow Dapper to directly work with Instants and LocalDateTimes from the Npgsql ADO.NET handler.
        // https://github.com/DapperLib/Dapper/issues/198#issuecomment-699719732
        SqlMapper.AddTypeMap(typeof(Instant), DbType.DateTime); // received_time: Instant <-> with timezone (UTC aligned)
        SqlMapper.AddTypeMap(typeof(LocalDateTime), DbType.DateTime2); // sps_zeit: LocalDateTime <-> without timezone (unknown timezone)
    }

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
