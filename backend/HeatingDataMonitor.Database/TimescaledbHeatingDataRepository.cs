using System.Data;
using Dapper;
using HeatingDataMonitor.Models;
using HeatingDataMonitor.Views;
using NodaTime;
using Npgsql;

namespace HeatingDataMonitor.Database;

internal class TimescaledbHeatingDataRepository : IHeatingDataRepository
{
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;

    public TimescaledbHeatingDataRepository(IConnectionProvider<NpgsqlConnection> connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public Task InsertRecordAsync(HeatingData heatingData) =>
        ExecuteAsync(@"INSERT INTO heating_data
VALUES (@SPS_Zeit, @ReceivedTime, @Kessel, @Ruecklauf, @Abgas, @CO2_Soll, @CO2_Ist, @Saugzug_Ist, @Puffer_Oben,
        @Puffer_Unten, @Platine, @Betriebsphase_Kessel, @Aussen, @Vorlauf_HK1_Ist, @Vorlauf_HK1_Soll,
        @Betriebsphase_HK1, @Vorlauf_HK2_Ist, @Vorlauf_HK2_Soll, @Betriebsphase_HK2, @Boiler_1, @DI_0, @DI_1, @DI_2,
        @DI_3, @A_W_0, @A_W_1, @A_W_2, @A_W_3, @A_EA_0, @A_EA_1, @A_EA_2, @A_EA_3, @A_EA_4, @A_PHASE_0, @A_PHASE_1,
        @A_PHASE_2, @A_PHASE_3, @A_PHASE_4);",
            heatingData);

    public Task<IEnumerable<HeatingData>> FetchAsync(Instant from, Instant to) =>
        QueryAsync<HeatingData>(
            @"SELECT * FROM heating_data WHERE received_time >= @from AND received_time < @to order by received_time;", new {from, to});

    public Task<IEnumerable<TemperatureChartData>> FetchMainTemperaturesAsync(Instant from, Instant to) =>
        QueryAsync<TemperatureChartData>(
            @"SELECT received_time, kessel, abgas, puffer_oben, puffer_unten, boiler_1 FROM heating_data WHERE received_time >= @from AND received_time < @to order by received_time;",
            new {from, to});

    private async Task<int> ExecuteAsync(string sql, object param)
    {
        await using NpgsqlConnection connection = await _connectionProvider.OpenConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    private async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param)
    {
        await using NpgsqlConnection connection = await _connectionProvider.OpenConnection();
        return await connection.QueryAsync<T>(sql, param);
    }
}
