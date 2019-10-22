using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;
using System.Globalization;

namespace DataHandler
{
    public class Data
    {
        #region Props

        #region General

        [DisplayableValue("Zeit")]
        public DateTime DatumZeit { get; set; }

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

        [DisplayableValue("Puffer Oben", Unit = "°C")]
        public float? Puffer_Oben { get; set; }

        [DisplayableValue("Puffer Unten", Unit = "°C")]
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

        [DisplayableValue("DI_0", IsRelevant = false)]
        public int? DI_0 { get; set; }

        [DisplayableValue("DI_1", IsRelevant = false)]
        public int? DI_1 { get; set; }

        [DisplayableValue("DI_2", IsRelevant = false)]
        public int? DI_2 { get; set; }

        [DisplayableValue("DI_3", IsRelevant = false)]
        public int? DI_3 { get; set; }

        [DisplayableValue("A_W_0", IsRelevant = false)]
        public int? A_W_0 { get; set; }

        [DisplayableValue("A_W_1", IsRelevant = false)]
        public int? A_W_1 { get; set; }

        [DisplayableValue("A_W_2", IsRelevant = false)]
        public int? A_W_2 { get; set; }

        [DisplayableValue("A_W_3", IsRelevant = false)]
        public int? A_W_3 { get; set; }

        [DisplayableValue("A_EA_0", IsRelevant = false)]
        public int? A_EA_0 { get; set; }

        [DisplayableValue("A_EA_1", IsRelevant = false)]
        public int? A_EA_1 { get; set; }

        [DisplayableValue("A_EA_2", IsRelevant = false)]
        public int? A_EA_2 { get; set; }

        [DisplayableValue("A_EA_3", IsRelevant = false)]
        public int? A_EA_3 { get; set; }

        [DisplayableValue("A_EA_4", IsRelevant = false)]
        public int? A_EA_4 { get; set; }

        [DisplayableValue("A_PHASE_0", IsRelevant = false)]
        public int? A_PHASE_0 { get; set; }

        [DisplayableValue("A_PHASE_1", IsRelevant = false)]
        public int? A_PHASE_1 { get; set; }

        [DisplayableValue("A_PHASE_2", IsRelevant = false)]
        public int? A_PHASE_2 { get; set; }

        [DisplayableValue("A_PHASE_3", IsRelevant = false)]
        public int? A_PHASE_3 { get; set; }

        [DisplayableValue("A_PHASE_4", IsRelevant = false)]
        public int? A_PHASE_4 { get; set; }

        #endregion

        #endregion

        #region Props (custom)

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Data_ID { get; private set; }

        private List<DisplayableValuePair>? _displayableListAll;
        private List<DisplayableValuePair>? _displayableListRelevant;

        [NotMapped]
        public List<DisplayableValuePair> DisplayableListAll
        {
            get
            {
                lock (this) // ensure SetDisplayableValues isn't called twice
                {
                    if (_displayableListAll == null)
                        SetDisplayableValues();
                }

                return _displayableListAll!;
            }
        }

        [NotMapped]
        public List<DisplayableValuePair> DisplayableListRelevant 
        {
            get
            {
                lock (this) // ensure SetDisplayableValues isn't called twice
                {
                    if (_displayableListRelevant == null)
                        SetDisplayableValues();
                }

                return _displayableListRelevant!;
            }
        }

        #endregion

        public void SetDisplayableValues()
        {
            _displayableListAll = new List<DisplayableValuePair>();
            _displayableListRelevant = new List<DisplayableValuePair>();
            // get all public instance properties
            IEnumerable<PropertyInfo> allProps = typeof(Data).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in allProps)
            {
                DisplayableValueAttribute attribute = property.GetCustomAttribute<DisplayableValueAttribute>();
                if (attribute == null) continue; // skip if there's no attribute

                Type actualType;
                // Get the type of the property
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

                // PropertyInfo.GetValue boxes the propertyvalue -> if it was a nullable without a value it was boxed to null, otherwise to the underlying stuct
                object? value = property.GetValue(this, null);

                // Get the string representation whilst taking in account the unit
                string stringRep = GetStringRep(actualType, value, attribute.Unit);

                // create the new value pair
                var dvp = new DisplayableValuePair(attribute.DisplayText, stringRep);

                // add to the lists
                DisplayableListAll.Add(dvp);
                if (attribute.IsRelevant) DisplayableListRelevant.Add(dvp);
            }
        }

        private string GetStringRep(Type valueType, object? boxedValue, string? unit)
        {
            if (boxedValue == null)
                return FormatExtensions.NullPlaceholder;

            if (valueType == typeof(BetriebsPhaseKessel))
                return ((BetriebsPhaseKessel)boxedValue).GetUserFirendlyString();
            if (valueType == typeof(BetriebsPhaseHK))
                return ((BetriebsPhaseHK)boxedValue).GetUserFirendlyString();

            if (valueType == typeof(DateTime))
                return ((DateTime)boxedValue).ToString("G", CultureInfo.CurrentCulture);

            return unit == null ? boxedValue.ToString() : $"{boxedValue.ToString()} {unit}";
        }

        public static Data FromSerialData(string serialData)
        {
            // not real data
            if (string.IsNullOrWhiteSpace(serialData))
                throw new FormatException("The serial data cannot be null or empty.");

            // split csv into fields
            string[] list = serialData.Split(';');

            // there should be 58 fields + one empty at the end because of the trailing ;
            if (list.Length != 59)
                throw new FormatException("The serial data doesn't contain 58 semicolons like expected for the 58 fields.");

            Data data;
            try
            {
                data = new Data
                {
                    DatumZeit = DateTime.TryParse($"{list[0]} {list[1]}", out DateTime vDatum) ? vDatum : throw new InvalidCastException("DateTime received from SerialPort is not valid"),
                    Kessel = list[2].ParseFloatOrNull(),
                    Ruecklauf = list[3].ParseFloatOrNull(),
                    Abgas = list[4].ParseFloatOrNull(),
                    Brennkammer = list[5].ParseFloatOrNull(),
                    CO2_Soll = list[6].ParseFloatOrNull(),
                    CO2_Ist = list[7].ParseFloatOrNull(),
                    Saugzug_Ist = list[8].ParseFloatOrNull(),
                    Puffer_Oben = list[9].ParseFloatOrNull(),
                    Puffer_Unten = list[10].ParseFloatOrNull(),
                    Platine = list[11].ParseFloatOrNull(),
                    Betriebsphase_Kessel = (BetriebsPhaseKessel?)list[12].ParseIntOrNull(),
                    Aussen = list[13].ParseFloatOrNull(),
                    Vorlauf_HK1_Ist = list[14].ParseFloatOrNull(),
                    Vorlauf_HK1_Soll = list[15].ParseFloatOrNull(),
                    Betriebsphase_HK1 = (BetriebsPhaseHK?)list[16].ParseIntOrNull(),
                    Betriebsart_Fern_HK1 = list[17].ParseIntOrNull(),
                    Verschiebung_Fern_HK1 = list[18].ParseFloatOrNull(),
                    Freigabekontakt_HK1 = list[19].ParseFloatOrNull(),
                    Vorlauf_HK2_Ist = list[20].ParseFloatOrNull(),
                    Vorlauf_HK2_Soll = list[21].ParseFloatOrNull(),
                    Betriebsphase_HK2 = (BetriebsPhaseHK?)list[22].ParseIntOrNull(),
                    Betriebsart_Fern_HK2 = list[23].ParseIntOrNull(),
                    Verschiebung_Fern_HK2 = list[24].ParseFloatOrNull(),
                    Freigabekontakt_HK2 = list[25].ParseFloatOrNull(),
                    Vorlauf_HK3_Ist = list[26].ParseFloatOrNull(),
                    Vorlauf_HK3_Soll = list[27].ParseFloatOrNull(),
                    Betriebsphase_HK3 = (BetriebsPhaseHK?)list[28].ParseIntOrNull(),
                    Betriebsart_Fern_HK3 = list[29].ParseIntOrNull(),
                    Verschiebung_Fern_HK3 = list[30].ParseFloatOrNull(),
                    Freigabekontakt_HK3 = list[31].ParseFloatOrNull(),
                    Vorlauf_HK4_Ist = list[32].ParseFloatOrNull(),
                    Vorlauf_HK4_Soll = list[33].ParseFloatOrNull(),
                    Betriebsphase_HK4 = (BetriebsPhaseHK?)list[34].ParseIntOrNull(),
                    Betriebsart_Fern_HK4 = list[35].ParseIntOrNull(),
                    Verschiebung_Fern_HK4 = list[36].ParseFloatOrNull(),
                    Freigabekontakt_HK4 = list[37].ParseFloatOrNull(),
                    Boiler_1 = list[38].ParseFloatOrNull(),
                    Boiler_2 = list[39].ParseFloatOrNull(),
                    DI_0 = list[40].ParseIntOrNull(),
                    DI_1 = list[41].ParseIntOrNull(),
                    DI_2 = list[42].ParseIntOrNull(),
                    DI_3 = list[43].ParseIntOrNull(),
                    A_W_0 = list[44].ParseIntOrNull(),
                    A_W_1 = list[45].ParseIntOrNull(),
                    A_W_2 = list[46].ParseIntOrNull(),
                    A_W_3 = list[47].ParseIntOrNull(),
                    A_EA_0 = list[48].ParseIntOrNull(),
                    A_EA_1 = list[49].ParseIntOrNull(),
                    A_EA_2 = list[50].ParseIntOrNull(),
                    A_EA_3 = list[51].ParseIntOrNull(),
                    A_EA_4 = list[52].ParseIntOrNull(),
                    A_PHASE_0 = list[53].ParseIntOrNull(),
                    A_PHASE_1 = list[54].ParseIntOrNull(),
                    A_PHASE_2 = list[55].ParseIntOrNull(),
                    A_PHASE_3 = list[56].ParseIntOrNull(),
                    A_PHASE_4 = list[57].ParseIntOrNull(),
                };
            }
            catch
            {
                throw new FormatException("The serial data couldn't be parsed correctly.");
            }

            return data;
        }
    }
}
