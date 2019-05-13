using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataHandler.Enums
{
    // int values are not according to order in specs (p. 44)
    public enum BetriebsPhaseKessel
    {
        Aus = 0,                // sure
        Anheizen = 1,           // sure
        Automatik = 2,          // sure
        Anheizen_erkennen = 3,
        Ausbrennen = 4,         // sure
        Gluterhaltung = 5,
        Abregeln = 6,           
        Uebertemperatur = 7,
        Tuer_Geoeffnet = 8
    }
}
