using DataHandler.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;
using static DataHandler.Extensions;
using DataHandler.Enums;

namespace DataHandler
{
    public class Data
    {
        private static Random rnd;

        #region Props

        #region General

        [DisplayableValue("Zeit")]
        public DateTime? DatumZeit { get; set; }

        [DisplayableValue("Kessel", Unit = "°C")]
        public float? Kessel { get; set; }

        [DisplayableValue("Rücklauf", Unit = "°C")]
        public float? Ruecklauf { get; set; }

        [DisplayableValue("Abgas", Unit = "°C")]
        public float? Abgas { get; set; }

        [DisplayableValue("Brennkammer", Unit = "°C")]
        public float? Brennkammer { get; set; }

        [DisplayableValue("CO2 Soll", Unit = "%")]
        public float? CO2_Soll { get; set; }

        [DisplayableValue("CO2 Ist", Unit = "%")]
        public float? CO2_Ist { get; set; }

        [DisplayableValue("Saugzug Ist", IsRelevant = false)]
        public float? Saugzug_Ist { get; set; }

        [DisplayableValue("Puffer Oben", Unit = "%")]
        public float? Puffer_Oben { get; set; }

        [DisplayableValue("Puffer Unten", Unit = "%")]
        public float? Puffer_Unten { get; set; }

        [DisplayableValue("Platine", IsRelevant = false, Unit = "°C")]
        public float? Platine { get; set; }

        [DisplayableValue("Betriebsphase Kessel")]
        public BetriebsPhaseKessel? Betriebsphase_Kessel { get; set; }

        [DisplayableValue("Aussen", Unit = "°C")]
        public float? Aussen { get; set; }

        #endregion

        #region HK1

        [DisplayableValue("Vorlauf HK1 Ist", Unit = "°C")]
        public float? Vorlauf_HK1_Ist { get; set; }

        [DisplayableValue("Vorlauf HK1 Soll", Unit = "°C")]
        public float? Vorlauf_HK1_Soll { get; set; }

        [DisplayableValue("Betriebsphase HK1")]
        public BetriebsPhaseHK? Betriebsphase_HK1 { get; set; }

        [DisplayableValue("Betriebsart Fern HK1", IsRelevant = false)]
        public int? Betriebsart_Fern_HK1 { get; set; }

        [DisplayableValue("Verschiebung Fern HK1", IsRelevant = false)]
        public float? Verschiebung_Fern_HK1 { get; set; }

        [DisplayableValue("Freigabekontakt HK1", IsRelevant = false)]
        public float? Freigabekontakt_HK1 { get; set; }

        #endregion
        #region HK2

        [DisplayableValue("Vorlauf HK2 Ist", Unit = "°C")]
        public float? Vorlauf_HK2_Ist { get; set; }

        [DisplayableValue("Vorlauf HK2 Soll", Unit = "°C")]
        public float? Vorlauf_HK2_Soll { get; set; }

        [DisplayableValue("Betriebsphase HK2")]
        public BetriebsPhaseHK? Betriebsphase_HK2 { get; set; }

        [DisplayableValue("Betriebsart Fern HK2", IsRelevant = false)]
        public int? Betriebsart_Fern_HK2 { get; set; }

        [DisplayableValue("Verschiebung Fern HK2", IsRelevant = false)]
        public float? Verschiebung_Fern_HK2 { get; set; }

        [DisplayableValue("Freigabekontakt HK2", IsRelevant = false)]
        public float? Freigabekontakt_HK2 { get; set; }

        #endregion
        #region HK3

        [DisplayableValue("Vorlauf HK3 Ist", Unit = "°C", IsRelevant = false)]
        public float? Vorlauf_HK3_Ist { get; set; }

        [DisplayableValue("Vorlauf HK3 Soll", Unit = "°C", IsRelevant = false)]
        public float? Vorlauf_HK3_Soll { get; set; }

        [DisplayableValue("Betriebsphase HK3", IsRelevant = false)]
        public BetriebsPhaseHK? Betriebsphase_HK3 { get; set; }

        [DisplayableValue("Betriebsart Fern HK3", IsRelevant = false)]
        public int? Betriebsart_Fern_HK3 { get; set; }

        [DisplayableValue("Verschiebung Fern HK3", IsRelevant = false)]
        public float? Verschiebung_Fern_HK3 { get; set; }

        [DisplayableValue("Freigabekontakt HK3", IsRelevant = false)]
        public float? Freigabekontakt_HK3 { get; set; }

        #endregion
        #region HK4

        [DisplayableValue("Vorlauf HK4 Ist", Unit = "°C", IsRelevant = false)]
        public float? Vorlauf_HK4_Ist { get; set; }

        [DisplayableValue("Vorlauf HK4 Soll", Unit = "°C", IsRelevant = false)]
        public float? Vorlauf_HK4_Soll { get; set; }

        [DisplayableValue("Betriebsphase HK4", IsRelevant = false)]
        public BetriebsPhaseHK? Betriebsphase_HK4 { get; set; }

        [DisplayableValue("Betriebsart Fern HK4", IsRelevant = false)]
        public int? Betriebsart_Fern_HK4 { get; set; }

        [DisplayableValue("Verschiebung Fern HK4", IsRelevant = false)]
        public float? Verschiebung_Fern_HK4 { get; set; }

        [DisplayableValue("Freigabekontakt HK4", IsRelevant = false)]
        public float? Freigabekontakt_HK4 { get; set; }

        #endregion

        #region Boilers

        [DisplayableValue("Boiler", Unit = "°C")]
        public float? Boiler_1 { get; set; }

        [DisplayableValue("Boiler 2", Unit = "°C", IsRelevant = false)]
        public float? Boiler_2 { get; set; }

        #endregion

        #region Unknown Values

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

        #endregion

        #region Props (custom)

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Data_ID { get; private set; }

        [NotMapped]
        public List<DisplayableValuePair> DisplayableListAll { get; private set; }

        [NotMapped]
        public List<DisplayableValuePair> DisplayableListRelevant { get; private set; }

        #endregion

        public void SetDisplayableValues()
        {
            DisplayableListAll = new List<DisplayableValuePair>();
            DisplayableListRelevant = new List<DisplayableValuePair>();
            // get all public instance properties
            IEnumerable<PropertyInfo> allProps = typeof(Data).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // select only those which have the DisplayableValueAttribute
            IEnumerable<PropertyInfo> displayableProps = allProps.Where(p => p.GetCustomAttributes<DisplayableValueAttribute>(false).Count() == 1);

            foreach (PropertyInfo property in displayableProps)
            {
                DisplayableValueAttribute attribute = property.GetCustomAttribute<DisplayableValueAttribute>();

                Type actualType;
                // Get the actual type of the property
                if (property.PropertyType.IsGenericTypeOf(typeof(Nullable<>)))
                {
                    // if it was a nullable, get the underlying type
                    actualType = Nullable.GetUnderlyingType(property.PropertyType);
                }
                else
                {
                    // otherwise just get the type
                    actualType = property.PropertyType;
                }

                // GetValue boxes the propertyvalue -> if it was a nullable it was boxed to null if it didn't have a value, otherwise to the underlying stuct
                Object value = property.GetValue(this, null);

                // Get the string representation whilst taking in account the unit
                string stringRep = GetStringRep(actualType, value, attribute.Unit);

                // create the new value pair
                var dvp = new DisplayableValuePair(attribute.DisplayText, stringRep);

                // add to the lists
                DisplayableListAll.Add(dvp);
                if (attribute.IsRelevant) DisplayableListRelevant.Add(dvp);
            }
        }

        private string GetStringRep(Type valueType, Object boxedValue, string unit)
        {
            if (boxedValue == null) return "#####";

            if (valueType == typeof(DateTime)) return ((DateTime)boxedValue).GetStringFromDateTime();
            if (valueType == typeof(BetriebsPhaseKessel)) return ((BetriebsPhaseKessel)boxedValue).GetUserFirendlyString();
            if (valueType == typeof(BetriebsPhaseHK)) return ((BetriebsPhaseHK)boxedValue).GetUserFirendlyString();

            return String.IsNullOrWhiteSpace(unit) ? boxedValue.ToString() : $"{boxedValue.ToString()} {unit}";
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
                    Betriebsphase_Kessel = int.TryParse(list[12], out int vBetriebsphase_Kessel) ? (BetriebsPhaseKessel?)vBetriebsphase_Kessel : null,
                    Aussen = float.TryParse(list[13], out float vAussen) ? (float?)vAussen : null,
                    Vorlauf_HK1_Ist = float.TryParse(list[14], out float vVorlauf_HK1_Ist) ? (float?)vVorlauf_HK1_Ist : null,
                    Vorlauf_HK1_Soll = float.TryParse(list[15], out float vVorlauf_HK1_Soll) ? (float?)vVorlauf_HK1_Soll : null,
                    Betriebsphase_HK1 = int.TryParse(list[16], out int vBetriebsphase_HK1) ? (BetriebsPhaseHK?)vBetriebsphase_HK1 : null,
                    Betriebsart_Fern_HK1 = int.TryParse(list[17], out int vBetriebsart_Fern_HK1) ? (int?)vBetriebsart_Fern_HK1 : null,
                    Verschiebung_Fern_HK1 = float.TryParse(list[18], out float vVerschiebung_Fern_HK1) ? (float?)vVerschiebung_Fern_HK1 : null,
                    Freigabekontakt_HK1 = float.TryParse(list[19], out float vFreigabekontakt_HK1) ? (float?)vFreigabekontakt_HK1 : null,
                    Vorlauf_HK2_Ist = float.TryParse(list[20], out float vVorlauf_HK2_Ist) ? (float?)vVorlauf_HK2_Ist : null,
                    Vorlauf_HK2_Soll = float.TryParse(list[21], out float vVorlauf_HK2_Soll) ? (float?)vVorlauf_HK2_Soll : null,
                    Betriebsphase_HK2 = int.TryParse(list[22], out int vBetriebsphase_HK2) ? (BetriebsPhaseHK?)vBetriebsphase_HK2 : null,
                    Betriebsart_Fern_HK2 = int.TryParse(list[23], out int vBetriebsart_Fern_HK2) ? (int?)vBetriebsart_Fern_HK2 : null,
                    Verschiebung_Fern_HK2 = float.TryParse(list[24], out float vVerschiebung_Fern_HK2) ? (float?)vVerschiebung_Fern_HK2 : null,
                    Freigabekontakt_HK2 = float.TryParse(list[25], out float vFreigabekontakt_HK2) ? (float?)vFreigabekontakt_HK2 : null,
                    Vorlauf_HK3_Ist = float.TryParse(list[26], out float vVorlauf_HK3_Ist) ? (float?)vVorlauf_HK3_Ist : null,
                    Vorlauf_HK3_Soll = float.TryParse(list[27], out float vVorlauf_HK3_Soll) ? (float?)vVorlauf_HK3_Soll : null,
                    Betriebsphase_HK3 = int.TryParse(list[28], out int vBetriebsphase_HK3) ? (BetriebsPhaseHK?)vBetriebsphase_HK3 : null,
                    Betriebsart_Fern_HK3 = int.TryParse(list[29], out int vBetriebsart_Fern_HK3) ? (int?)vBetriebsart_Fern_HK3 : null,
                    Verschiebung_Fern_HK3 = float.TryParse(list[30], out float vVerschiebung_Fern_HK3) ? (float?)vVerschiebung_Fern_HK3 : null,
                    Freigabekontakt_HK3 = float.TryParse(list[31], out float vFreigabekontakt_HK3) ? (float?)vFreigabekontakt_HK3 : null,
                    Vorlauf_HK4_Ist = float.TryParse(list[32], out float vVorlauf_HK4_Ist) ? (float?)vVorlauf_HK4_Ist : null,
                    Vorlauf_HK4_Soll = float.TryParse(list[33], out float vVorlauf_HK4_Soll) ? (float?)vVorlauf_HK4_Soll : null,
                    Betriebsphase_HK4 = int.TryParse(list[34], out int vBetriebsphase_HK4) ? (BetriebsPhaseHK?)vBetriebsphase_HK4 : null,
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

            Array phasenKessel = Enum.GetValues(typeof(BetriebsPhaseKessel));
            Array phasenHK = Enum.GetValues(typeof(BetriebsPhaseHK));

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
                Betriebsphase_Kessel = (BetriebsPhaseKessel)phasenKessel.GetValue(rnd.Next(phasenKessel.Length)),
                Aussen = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK1_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK1_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK1 = (BetriebsPhaseHK)phasenHK.GetValue(rnd.Next(phasenHK.Length)),
                Betriebsart_Fern_HK1 = rnd.Next(500),
                Verschiebung_Fern_HK1 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK1 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK2_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK2_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK2 = (BetriebsPhaseHK)phasenHK.GetValue(rnd.Next(phasenHK.Length)),
                Betriebsart_Fern_HK2 = rnd.Next(500),
                Verschiebung_Fern_HK2 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK2 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK3_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK3_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK3 = (BetriebsPhaseHK)phasenHK.GetValue(rnd.Next(phasenHK.Length)),
                Betriebsart_Fern_HK3 = rnd.Next(500),
                Verschiebung_Fern_HK3 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK3 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK4_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK4_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK4 = (BetriebsPhaseHK)phasenHK.GetValue(rnd.Next(phasenHK.Length)),
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
