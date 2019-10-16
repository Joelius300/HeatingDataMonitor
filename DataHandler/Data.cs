using DataHandler.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;
using DataHandler.Enums;

namespace DataHandler
{
    public class Data
    {
        private static Random rnd;

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
                object value = property.GetValue(this, null);

                // Get the string representation whilst taking in account the unit
                string stringRep = GetStringRep(actualType, value, attribute.Unit);

                // create the new value pair
                var dvp = new DisplayableValuePair(attribute.DisplayText, stringRep);

                // add to the lists
                DisplayableListAll.Add(dvp);
                if (attribute.IsRelevant) DisplayableListRelevant.Add(dvp);
            }
        }

        private string GetStringRep(Type valueType, object boxedValue, string unit)
        {
            if (boxedValue == null) return "#####";

            if (valueType == typeof(DateTime)) return ((DateTime)boxedValue).GetStringFromDateTime();
            if (valueType == typeof(BetriebsPhaseKessel)) return ((BetriebsPhaseKessel)boxedValue).GetUserFirendlyString();
            if (valueType == typeof(BetriebsPhaseHK)) return ((BetriebsPhaseHK)boxedValue).GetUserFirendlyString();

            return String.IsNullOrWhiteSpace(unit) ? boxedValue.ToString() : $"{boxedValue.ToString()} {unit}";
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
            data.SetDisplayableValues();

            return data;
        }
    }
}
