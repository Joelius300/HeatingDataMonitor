using NodaTime;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IdentifierTypo

namespace HeatingDataMonitor.Data.Model;

// TODO We could use Postgres Enums instead of shorts which gives us
// nicer queries in SQL and probably also some very small perf gain idk tho.
// https://www.npgsql.org/doc/types/enums_and_composites.html
public class HeatingData
{
    public LocalDateTime SPS_Zeit { get; set; }
    public Instant ReceivedTime { get; set; }
    public float? Kessel { get; set; }
    public float? Ruecklauf { get; set; }
    public float? Abgas { get; set; }
    public float? CO2_Soll { get; set; }
    public float? CO2_Ist { get; set; }
    public float? Saugzug_Ist { get; set; }
    public float? Puffer_Oben { get; set; }
    public float? Puffer_Unten { get; set; }
    public float? Platine { get; set; }
    public BetriebsPhaseKessel? Betriebsphase_Kessel { get; set; }
    public float? Aussen { get; set; }
    public float? Vorlauf_HK1_Ist { get; set; }
    public float? Vorlauf_HK1_Soll { get; set; }
    public BetriebsPhaseHK? Betriebsphase_HK1 { get; set; }
    public float? Vorlauf_HK2_Ist { get; set; }
    public float? Vorlauf_HK2_Soll { get; set; }
    public BetriebsPhaseHK? Betriebsphase_HK2 { get; set; }
    public float? Boiler_1 { get; set; }
    public short? DI_0 { get; set; }
    public short? DI_1 { get; set; }
    public short? DI_2 { get; set; }
    public short? DI_3 { get; set; }
    public short? A_W_0 { get; set; }
    public short? A_W_1 { get; set; }
    public short? A_W_2 { get; set; }
    public short? A_W_3 { get; set; }
    public short? A_EA_0 { get; set; }
    public short? A_EA_1 { get; set; }
    public short? A_EA_2 { get; set; }
    public short? A_EA_3 { get; set; }
    public short? A_EA_4 { get; set; }
    public short? A_PHASE_0 { get; set; }
    public short? A_PHASE_1 { get; set; }
    public short? A_PHASE_2 { get; set; }
    public short? A_PHASE_3 { get; set; }
    public short? A_PHASE_4 { get; set; }
}
