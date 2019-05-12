using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataHandler.Enums
{
    public enum BetriebsPhaseKessel
    {
        Aus = 0,
        [Description("Anheizen erkennen")]
        Anheizen_erkennen = 1,
        Anheizen = 2,
        Automatik = 3,
        Abregeln = 4,
        Gluterhaltung = 5,
        Ausbrennen = 6,
        [Description("Übertemperatur")]
        Uebertemperatur = 7,
        [Description("Tür geöffnet")]
        Tuer_Geoeffnet = 8
    }
}
