using Dapper;
using HeatingDataMonitor.Database.Models;
using Npgsql;

namespace HeatingDataMonitor.Database.Write;

internal class TimescaledbHeatingDataWriteRepository : IHeatingDataWriteRepository
{
    private readonly IConnectionProvider<NpgsqlConnection> _connectionProvider;

    public TimescaledbHeatingDataWriteRepository(IConnectionProvider<NpgsqlConnection> connectionProvider)
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
    private async Task<int> ExecuteAsync(string sql, object param)
    {
        await using NpgsqlConnection connection = await _connectionProvider.OpenConnection();
        return await connection.ExecuteAsync(sql, param);
    }
}
