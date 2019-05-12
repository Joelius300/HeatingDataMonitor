using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Enums
{
    public static class EnumExtensions
    {
        public static string GetUserFirendlyString(this BetriebsPhaseKessel value)
        {
            switch (value)
            {
                case BetriebsPhaseKessel.Anheizen_erkennen:
                    return "Anheizen erkennen";
                case BetriebsPhaseKessel.Uebertemperatur:
                    return "Übertemperatur";
                case BetriebsPhaseKessel.Tuer_Geoeffnet:
                    return "Tür geöffnet";
            }

            return value.ToString();
        }

        public static string GetUserFirendlyString(this BetriebsPhaseHK value) {
            switch (value)
            {
                case BetriebsPhaseHK.Unter_Freigabetemperatur:
                    return "Unter Freigabetemperatur";
            }

            return value.ToString();
        }
    }
}
