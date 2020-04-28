using System;

namespace HeatingDataMonitor.Model
{
    public class HeatingData
    {
        public int Id { get; set; }

        public DateTime Zeit { get; set; }
        public float? Kessel { get; set; }
        public float? Ruecklauf { get; set; }
        public float? Abgas { get; set; }
        public float? Brennkammer { get; set; }
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
        public int? Betriebsart_Fern_HK1 { get; set; }
        public float? Verschiebung_Fern_HK1 { get; set; }
        public float? Freigabekontakt_HK1 { get; set; }
        public float? Vorlauf_HK2_Ist { get; set; }
        public float? Vorlauf_HK2_Soll { get; set; }
        public BetriebsPhaseHK? Betriebsphase_HK2 { get; set; }
        public int? Betriebsart_Fern_HK2 { get; set; }
        public float? Verschiebung_Fern_HK2 { get; set; }
        public float? Freigabekontakt_HK2 { get; set; }
        public float? Vorlauf_HK3_Ist { get; set; }
        public float? Vorlauf_HK3_Soll { get; set; }
        public BetriebsPhaseHK? Betriebsphase_HK3 { get; set; }
        public int? Betriebsart_Fern_HK3 { get; set; }
        public float? Verschiebung_Fern_HK3 { get; set; }
        public float? Freigabekontakt_HK3 { get; set; }
        public float? Vorlauf_HK4_Ist { get; set; }
        public float? Vorlauf_HK4_Soll { get; set; }
        public BetriebsPhaseHK? Betriebsphase_HK4 { get; set; }
        public int? Betriebsart_Fern_HK4 { get; set; }
        public float? Verschiebung_Fern_HK4 { get; set; }
        public float? Freigabekontakt_HK4 { get; set; }
        public float? Boiler_1 { get; set; }
        public float? Boiler_2 { get; set; }
        public int? DI_0 { get; set; }
        public int? DI_1 { get; set; }
        public int? DI_2 { get; set; }
        public int? DI_3 { get; set; }
        public int? A_W_0 { get; set; }
        public int? A_W_1 { get; set; }
        public int? A_W_2 { get; set; }
        public int? A_W_3 { get; set; }
        public int? A_EA_0 { get; set; }
        public int? A_EA_1 { get; set; }
        public int? A_EA_2 { get; set; }
        public int? A_EA_3 { get; set; }
        public int? A_EA_4 { get; set; }
        public int? A_PHASE_0 { get; set; }
        public int? A_PHASE_1 { get; set; }
        public int? A_PHASE_2 { get; set; }
        public int? A_PHASE_3 { get; set; }
        public int? A_PHASE_4 { get; set; }
    }
}
