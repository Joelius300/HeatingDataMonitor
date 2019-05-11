using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHandler
{
    public class Data
    {
        private static Random rnd;

        #region Props (from CSV)
        public DateTime? DatumZeit { get; set; }
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
        public int? Betriebsphase_Kessel { get; set; }
        public float? Aussen { get; set; }
        public float? Vorlauf_HK1_Ist { get; set; }
        public float? Vorlauf_HK1_Soll { get; set; }
        public int? Betriebsphase_HK1 { get; set; }
        public int? Betriebsart_Fern_HK1 { get; set; }
        public float? Verschiebung_Fern_HK1 { get; set; }
        public float? Freigabekontakt_HK1 { get; set; }
        public float? Vorlauf_HK2_Ist { get; set; }
        public float? Vorlauf_HK2_Soll { get; set; }
        public int? Betriebsphase_HK2 { get; set; }
        public int? Betriebsart_Fern_HK2 { get; set; }
        public float? Verschiebung_Fern_HK2 { get; set; }
        public float? Freigabekontakt_HK2 { get; set; }
        public float? Vorlauf_HK3_Ist { get; set; }
        public float? Vorlauf_HK3_Soll { get; set; }
        public int? Betriebsphase_HK3 { get; set; }
        public int? Betriebsart_Fern_HK3 { get; set; }
        public float? Verschiebung_Fern_HK3 { get; set; }
        public float? Freigabekontakt_HK3 { get; set; }
        public float? Vorlauf_HK4_Ist { get; set; }
        public float? Vorlauf_HK4_Soll { get; set; }
        public int? Betriebsphase_HK4 { get; set; }
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

        #endregion

        #region Props (custom)

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Data_ID { get; private set; }

        [NotMapped]
        public List<(string Header, string Data)> DisplayableList { get; private set; }

        #endregion

        public void SetDisplayableValues()
        {
            DisplayableList = new List<(string Header, string Data)>()
            {
                ("Zeit", DatumZeit.Value.ToString("dd.MM.yyyy HH:mm:ss")),
                ("Kessel", Kessel.GetString("°C")),
                ("Rücklauf", Ruecklauf.GetString("°C")),
                ("Abgas", Abgas.GetString("°C")),
                ("Brennkammer", Brennkammer.GetString("°C")),
                ("CO2 Soll", CO2_Soll.GetString("%")),
                ("CO2 Ist", CO2_Ist.GetString("%")),
                ("Saugzug Ist", Saugzug_Ist.GetString("rpm")),
                ("Puffer oben", Puffer_Oben.GetString("°C")),
                ("Puffer unten", Puffer_Unten.GetString("°C")),
                ("Platine", Platine.GetString("°C")),
                ("Betriebsphase Kessel", Betriebsphase_Kessel.GetString()),
                ("Aussen", Aussen.GetString("°C")),
                ("Vorlauf HK1 Ist", Vorlauf_HK1_Ist.GetString("°C")),
                ("Vorlauf HK1 Soll", Vorlauf_HK1_Soll.GetString("°C")),
                ("Betriebsphase HK1", Betriebsphase_HK1.GetString()),
                ("Betriebsart Fern HK1", Betriebsart_Fern_HK1.GetString()),
                ("Verschiebung Fern HK1", Verschiebung_Fern_HK1.GetString()),
                ("Freigabekontakt HK1", Freigabekontakt_HK1.GetString()),
                ("Vorlauf HK2 Ist", Vorlauf_HK2_Ist.GetString("°C")),
                ("Vorlauf HK2 Soll", Vorlauf_HK2_Soll.GetString("°C")),
                ("Betriebsphase HK2", Betriebsphase_HK2.GetString()),
                ("Betriebsart Fern HK2", Betriebsart_Fern_HK2.GetString()),
                ("Verschiebung Fern HK2", Verschiebung_Fern_HK2.GetString()),
                ("Freigabekontakt HK2", Freigabekontakt_HK2.GetString()),
                ("Vorlauf HK3 Ist", Vorlauf_HK3_Ist.GetString("°C")),
                ("Vorlauf HK3 Soll", Vorlauf_HK3_Soll.GetString("°C")),
                ("Betriebsphase HK3", Betriebsphase_HK3.GetString()),
                ("Betriebsart Fern HK3", Betriebsart_Fern_HK3.GetString()),
                ("Verschiebung Fern HK3", Verschiebung_Fern_HK3.GetString()),
                ("Freigabekontakt HK3", Freigabekontakt_HK3.GetString()),
                ("Vorlauf HK4 Ist", Vorlauf_HK4_Ist.GetString("°C")),
                ("Vorlauf HK4 Soll", Vorlauf_HK4_Soll.GetString("°C")),
                ("Betriebsphase HK4", Betriebsphase_HK4.GetString()),
                ("Betriebsart Fern HK4", Betriebsart_Fern_HK4.GetString()),
                ("Verschiebung Fern HK4", Verschiebung_Fern_HK4.GetString()),
                ("Freigabekontakt HK4", Freigabekontakt_HK4.GetString()),
                ("Boiler 1", Boiler_1.GetString("°C")),
                ("Boiler 2", Boiler_2.GetString("°C")),
                ("DI 0", DI_0.GetString()),
                ("DI 1", DI_1.GetString()),
                ("DI 2", DI_2.GetString()),
                ("DI 3", DI_3.GetString()),
                ("A_W 0", A_W_0.GetString()),
                ("A_W 1", A_W_1.GetString()),
                ("A_W 2", A_W_2.GetString()),
                ("A_W 3", A_W_3.GetString()),
                ("A_EA 0", A_EA_0.GetString()),
                ("A_EA 1", A_EA_1.GetString()),
                ("A_EA 2", A_EA_2.GetString()),
                ("A_EA 3", A_EA_3.GetString()),
                ("A_EA 4", A_EA_4.GetString()),
                ("A_Phase 0", A_PHASE_0.GetString()),
                ("A_Phase 1", A_PHASE_1.GetString()),
                ("A_Phase 2", A_PHASE_2.GetString()),
                ("A_Phase 3", A_PHASE_3.GetString()),
                ("A_Phase 4", A_PHASE_4.GetString())
            };
        }

        public void CalcCustomProps()
        {
            SetDisplayableValues();
        }

        public static Data Convert(string serialData)
        {
            // not real data
            if (String.IsNullOrWhiteSpace(serialData)) {
                return null;
            }

            // split csv into fields
            string[] list = serialData.Split(';');

            // there should be 58 fields + one empty at the end because of the trailing ;
            if (list.Length != 59) {
                return null;
            }

            Data data;
            try
            {
                data = new Data
                {
                    DatumZeit = DateTime.TryParse($"{list[0]} {list[1]}", out DateTime vDatum) ? (DateTime?)vDatum : null,
                    Kessel = float.TryParse(list[2], out float vKessel) ? (float?)vKessel : null,
                    Ruecklauf = float.TryParse(list[3], out float vRuecklauf) ? (float?)vRuecklauf : null,
                    Abgas = float.TryParse(list[4], out float vAbgas) ? (float?)vAbgas : null,
                    Brennkammer = float.TryParse(list[5], out float vBrennkammer) ? (float?)vBrennkammer : null,
                    CO2_Soll = float.TryParse(list[6], out float vCO2_Soll) ? (float?)vCO2_Soll : null,
                    CO2_Ist = float.TryParse(list[7], out float vCO2_Ist) ? (float?)vCO2_Ist : null,
                    Saugzug_Ist = float.TryParse(list[8], out float vSaugzug_Ist) ? (float?)vSaugzug_Ist : null,
                    Puffer_Oben = float.TryParse(list[9], out float vPuffer_Oben) ? (float?)vPuffer_Oben : null,
                    Puffer_Unten = float.TryParse(list[10], out float vPuffer_Unten) ? (float?)vPuffer_Unten : null,
                    Platine = float.TryParse(list[11], out float vPlatine) ? (float?)vPlatine : null,
                    Betriebsphase_Kessel = int.TryParse(list[12], out int vBetriebsphase_Kessel) ? (int?)vBetriebsphase_Kessel : null,
                    Aussen = float.TryParse(list[13], out float vAussen) ? (float?)vAussen : null,
                    Vorlauf_HK1_Ist = float.TryParse(list[14], out float vVorlauf_HK1_Ist) ? (float?)vVorlauf_HK1_Ist : null,
                    Vorlauf_HK1_Soll = float.TryParse(list[15], out float vVorlauf_HK1_Soll) ? (float?)vVorlauf_HK1_Soll : null,
                    Betriebsphase_HK1 = int.TryParse(list[16], out int vBetriebsphase_HK1) ? (int?)vBetriebsphase_HK1 : null,
                    Betriebsart_Fern_HK1 = int.TryParse(list[17], out int vBetriebsart_Fern_HK1) ? (int?)vBetriebsart_Fern_HK1 : null,
                    Verschiebung_Fern_HK1 = float.TryParse(list[18], out float vVerschiebung_Fern_HK1) ? (float?)vVerschiebung_Fern_HK1 : null,
                    Freigabekontakt_HK1 = float.TryParse(list[19], out float vFreigabekontakt_HK1) ? (float?)vFreigabekontakt_HK1 : null,
                    Vorlauf_HK2_Ist = float.TryParse(list[20], out float vVorlauf_HK2_Ist) ? (float?)vVorlauf_HK2_Ist : null,
                    Vorlauf_HK2_Soll = float.TryParse(list[21], out float vVorlauf_HK2_Soll) ? (float?)vVorlauf_HK2_Soll : null,
                    Betriebsphase_HK2 = int.TryParse(list[22], out int vBetriebsphase_HK2) ? (int?)vBetriebsphase_HK2 : null,
                    Betriebsart_Fern_HK2 = int.TryParse(list[23], out int vBetriebsart_Fern_HK2) ? (int?)vBetriebsart_Fern_HK2 : null,
                    Verschiebung_Fern_HK2 = float.TryParse(list[24], out float vVerschiebung_Fern_HK2) ? (float?)vVerschiebung_Fern_HK2 : null,
                    Freigabekontakt_HK2 = float.TryParse(list[25], out float vFreigabekontakt_HK2) ? (float?)vFreigabekontakt_HK2 : null,
                    Vorlauf_HK3_Ist = float.TryParse(list[26], out float vVorlauf_HK3_Ist) ? (float?)vVorlauf_HK3_Ist : null,
                    Vorlauf_HK3_Soll = float.TryParse(list[27], out float vVorlauf_HK3_Soll) ? (float?)vVorlauf_HK3_Soll : null,
                    Betriebsphase_HK3 = int.TryParse(list[28], out int vBetriebsphase_HK3) ? (int?)vBetriebsphase_HK3 : null,
                    Betriebsart_Fern_HK3 = int.TryParse(list[29], out int vBetriebsart_Fern_HK3) ? (int?)vBetriebsart_Fern_HK3 : null,
                    Verschiebung_Fern_HK3 = float.TryParse(list[30], out float vVerschiebung_Fern_HK3) ? (float?)vVerschiebung_Fern_HK3 : null,
                    Freigabekontakt_HK3 = float.TryParse(list[31], out float vFreigabekontakt_HK3) ? (float?)vFreigabekontakt_HK3 : null,
                    Vorlauf_HK4_Ist = float.TryParse(list[32], out float vVorlauf_HK4_Ist) ? (float?)vVorlauf_HK4_Ist : null,
                    Vorlauf_HK4_Soll = float.TryParse(list[33], out float vVorlauf_HK4_Soll) ? (float?)vVorlauf_HK4_Soll : null,
                    Betriebsphase_HK4 = int.TryParse(list[34], out int vBetriebsphase_HK4) ? (int?)vBetriebsphase_HK4 : null,
                    Betriebsart_Fern_HK4 = int.TryParse(list[35], out int vBetriebsart_Fern_HK4) ? (int?)vBetriebsart_Fern_HK4 : null,
                    Verschiebung_Fern_HK4 = float.TryParse(list[36], out float vVerschiebung_Fern_HK4) ? (float?)vVerschiebung_Fern_HK4 : null,
                    Freigabekontakt_HK4 = float.TryParse(list[37], out float vFreigabekontakt_HK4) ? (float?)vFreigabekontakt_HK4 : null,
                    Boiler_1 = float.TryParse(list[38], out float vBoiler_1) ? (float?)vBoiler_1 : null,
                    Boiler_2 = float.TryParse(list[39], out float vBoiler_2) ? (float?)vBoiler_2 : null,
                    DI_0 = int.TryParse(list[40], out int vDI_0) ? (int?)vDI_0 : null,
                    DI_1 = int.TryParse(list[41], out int vDI_1) ? (int?)vDI_1 : null,
                    DI_2 = int.TryParse(list[42], out int vDI_2) ? (int?)vDI_2 : null,
                    DI_3 = int.TryParse(list[43], out int vDI_3) ? (int?)vDI_3 : null,
                    A_W_0 = int.TryParse(list[44], out int vA_W_0) ? (int?)vA_W_0 : null,
                    A_W_1 = int.TryParse(list[45], out int vA_W_1) ? (int?)vA_W_1 : null,
                    A_W_2 = int.TryParse(list[46], out int vA_W_2) ? (int?)vA_W_2 : null,
                    A_W_3 = int.TryParse(list[47], out int vA_W_3) ? (int?)vA_W_3 : null,
                    A_EA_0 = int.TryParse(list[48], out int vA_EA_0) ? (int?)vA_EA_0 : null,
                    A_EA_1 = int.TryParse(list[49], out int vA_EA_1) ? (int?)vA_EA_1 : null,
                    A_EA_2 = int.TryParse(list[50], out int vA_EA_2) ? (int?)vA_EA_2 : null,
                    A_EA_3 = int.TryParse(list[51], out int vA_EA_3) ? (int?)vA_EA_3 : null,
                    A_EA_4 = int.TryParse(list[52], out int vA_EA_4) ? (int?)vA_EA_4 : null,
                    A_PHASE_0 = int.TryParse(list[53], out int vA_PHASE_0) ? (int?)vA_PHASE_0 : null,
                    A_PHASE_1 = int.TryParse(list[54], out int vA_PHASE_1) ? (int?)vA_PHASE_1 : null,
                    A_PHASE_2 = int.TryParse(list[55], out int vA_PHASE_2) ? (int?)vA_PHASE_2 : null,
                    A_PHASE_3 = int.TryParse(list[56], out int vA_PHASE_3) ? (int?)vA_PHASE_3 : null,
                    A_PHASE_4 = int.TryParse(list[57], out int vA_PHASE_4) ? (int?)vA_PHASE_4 : null
                };

                data.CalcCustomProps();
            }
            catch
            {
                return null;
            }

            return data;

            #region old bad way

            //Data data = new Data
            //{
            //    Datum = DateTime.Parse(list[0]),
            //    Uhrzeit = DateTime.Parse(list[1]),
            //    Kessel = float.Parse(list[2]),
            //    Ruecklauf = float.Parse(list[3]),
            //    Abgas = float.Parse(list[4]),
            //    Brennkammer = float.Parse(list[5]),
            //    CO2_Soll = float.Parse(list[6]),
            //    CO2_Ist = float.Parse(list[7]),
            //    Saugzug_Ist = float.Parse(list[8]),
            //    Puffer_Oben = float.Parse(list[9]),
            //    Puffer_Unten = float.Parse(list[10]),
            //    Platine = float.Parse(list[11]),
            //    Betriebsphase_Kessel = int.Parse(list[12]),
            //    Aussen = float.Parse(list[13]),
            //    Vorlauf_HK1_Ist = float.Parse(list[14]),
            //    Vorlauf_HK1_Soll = float.Parse(list[15]),
            //    Betriebsphase_HK1 = int.Parse(list[16]),
            //    Betriebsart_Fern_HK1 = int.Parse(list[17]),
            //    Verschiebung_Fern_HK1 = float.Parse(list[18]),
            //    Freigabekontakt_HK1 = float.Parse(list[19]),
            //    Vorlauf_HK2_Ist = float.Parse(list[20]),
            //    Vorlauf_HK2_Soll = float.Parse(list[21]),
            //    Betriebsphase_HK2 = int.Parse(list[22]),
            //    Betriebsart_Fern_HK2 = int.Parse(list[23]),
            //    Verschiebung_Fern_HK2 = float.Parse(list[24]),
            //    Freigabekontakt_HK2 = float.Parse(list[25]),
            //    Vorlauf_HK3_Ist = float.Parse(list[26]),
            //    Vorlauf_HK3_Soll = float.Parse(list[27]),
            //    Betriebsphase_HK3 = int.Parse(list[28]),
            //    Betriebsart_Fern_HK3 = int.Parse(list[29]),
            //    Verschiebung_Fern_HK3 = float.Parse(list[30]),
            //    Freigabekontakt_HK3 = float.Parse(list[31]),
            //    Vorlauf_HK4_Ist = float.Parse(list[32]),
            //    Vorlauf_HK4_Soll = float.Parse(list[33]),
            //    Betriebsphase_HK4 = int.Parse(list[34]),
            //    Betriebsart_Fern_HK4 = int.Parse(list[35]),
            //    Verschiebung_Fern_HK4 = float.Parse(list[36]),
            //    Freigabekontakt_HK4 = float.Parse(list[37]),
            //    Boiler_1 = float.Parse(list[38]),
            //    Boiler_2 = float.Parse(list[39]),
            //    DI_0 = int.Parse(list[40]),
            //    DI_1 = int.Parse(list[41]),
            //    DI_2 = int.Parse(list[42]),
            //    DI_3 = int.Parse(list[43]),
            //    A_W_0 = int.Parse(list[44]),
            //    A_W_1 = int.Parse(list[45]),
            //    A_W_2 = int.Parse(list[46]),
            //    A_W_3 = int.Parse(list[47]),
            //    A_EA_0 = int.Parse(list[48]),
            //    A_EA_1 = int.Parse(list[49]),
            //    A_EA_2 = int.Parse(list[50]),
            //    A_EA_3 = int.Parse(list[51]),
            //    A_EA_4 = int.Parse(list[52]),
            //    A_PHASE_0 = int.Parse(list[53]),
            //    A_PHASE_1 = int.Parse(list[54]),
            //    A_PHASE_2 = int.Parse(list[55]),
            //    A_PHASE_3 = int.Parse(list[56]),
            //    A_PHASE_4 = int.Parse(list[57])
            //};

            #endregion
        }

        public static Data GetRandomData()
        {
            if (rnd == null) rnd = new Random();

            Data data = new Data
            {
                DatumZeit = DateTime.UtcNow,
                Kessel = float.Parse(rnd.NextDouble().ToString()),
                Ruecklauf = float.Parse(rnd.NextDouble().ToString()),
                Abgas = float.Parse(rnd.NextDouble().ToString()),
                Brennkammer = float.Parse(rnd.NextDouble().ToString()),
                CO2_Soll = float.Parse(rnd.NextDouble().ToString()),
                CO2_Ist = float.Parse(rnd.NextDouble().ToString()),
                Saugzug_Ist = float.Parse(rnd.NextDouble().ToString()),
                Puffer_Oben = float.Parse(rnd.NextDouble().ToString()),
                Puffer_Unten = float.Parse(rnd.NextDouble().ToString()),
                Platine = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_Kessel = rnd.Next(500),
                Aussen = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK1_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK1_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK1 = rnd.Next(500),
                Betriebsart_Fern_HK1 = rnd.Next(500),
                Verschiebung_Fern_HK1 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK1 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK2_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK2_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK2 = rnd.Next(500),
                Betriebsart_Fern_HK2 = rnd.Next(500),
                Verschiebung_Fern_HK2 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK2 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK3_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK3_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK3 = rnd.Next(500),
                Betriebsart_Fern_HK3 = rnd.Next(500),
                Verschiebung_Fern_HK3 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK3 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK4_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK4_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK4 = rnd.Next(500),
                Betriebsart_Fern_HK4 = rnd.Next(500),
                Verschiebung_Fern_HK4 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK4 = float.Parse(rnd.NextDouble().ToString()),
                Boiler_1 = float.Parse(rnd.NextDouble().ToString()),
                Boiler_2 = float.Parse(rnd.NextDouble().ToString()),
                DI_0 = rnd.Next(500),
                DI_1 = rnd.Next(500),
                DI_2 = rnd.Next(500),
                DI_3 = rnd.Next(500),
                A_W_0 = rnd.Next(500),
                A_W_1 = rnd.Next(500),
                A_W_2 = rnd.Next(500),
                A_W_3 = rnd.Next(500),
                A_EA_0 = rnd.Next(500),
                A_EA_1 = rnd.Next(500),
                A_EA_2 = rnd.Next(500),
                A_EA_3 = rnd.Next(500),
                A_EA_4 = rnd.Next(500),
                A_PHASE_0 = rnd.Next(500),
                A_PHASE_1 = rnd.Next(500),
                A_PHASE_2 = rnd.Next(500),
                A_PHASE_3 = rnd.Next(500),
                A_PHASE_4 = rnd.Next(500)
            };
            data.CalcCustomProps();

            return data;
        }
    }
}
