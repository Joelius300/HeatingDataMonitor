using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataHandler.Enums
{
    // int values are not according to order in specs (p. 44)
    public enum BetriebsPhaseKessel
    {
        Aus = 0,
        Anheizen = 1,
        Automatik = 2,
        Anheizen_erkennen = 3,
        Abregeln = 4,
        Gluterhaltung = 5,
        Ausbrennen = 6,
        Uebertemperatur = 7,
        Tuer_Geoeffnet = 8
    }
}
