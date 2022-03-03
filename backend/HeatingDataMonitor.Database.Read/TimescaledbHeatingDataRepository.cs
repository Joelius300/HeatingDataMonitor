using Dapper;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Database.Views;
using NodaTime;
using Npgsql;

namespace HeatingDataMonitor.Database.Read;

internal class TimescaledbHeatingDataRepository : IHeatingDataRepository
{
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;

    public TimescaledbHeatingDataRepository(IConnectionProvider<NpgsqlConnection> connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public Task<IEnumerable<HeatingData>> FetchAsync(Instant from, Instant to) =>
        QueryAsync<HeatingData>(
            @"SELECT * FROM heating_data WHERE received_time >= @from AND received_time < @to order by received_time;",
            new {from, to});

    public Task<IEnumerable<TemperatureChartData>> FetchMainTemperaturesAsync(Instant from, Instant to) =>
        QueryAsync<TemperatureChartData>(
            @"SELECT received_time, kessel, abgas, puffer_oben, puffer_unten, boiler_1 FROM heating_data WHERE received_time >= @from AND received_time < @to order by received_time;",
            new {from, to});

    private async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param)
    {
        await using NpgsqlConnection connection = await _connectionProvider.OpenConnection();
        return await connection.QueryAsync<T>(sql, param);
    }
}
