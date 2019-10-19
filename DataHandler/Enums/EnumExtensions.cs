using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Enums
{
    public static class EnumExtensions
    {
        public static string GetUserFirendlyString(this BetriebsPhaseKessel value) =>
            value switch
            {
                BetriebsPhaseKessel.Anheizen_erkennen => "Anheizen erkennen",
                BetriebsPhaseKessel.Uebertemperatur => "Übertemperatur",
                BetriebsPhaseKessel.Tuer_Geoeffnet => "Tür geöffnet",

                _ => value.ToString(),
            };

        public static string GetUserFirendlyString(this BetriebsPhaseHK value) =>
            value switch
            {
                BetriebsPhaseHK.Unter_Freigabetemperatur => "Unter Freigabetemperatur",

                _ => value.ToString(),
            };
    }
}
