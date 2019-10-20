using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataHandler
{
    // int values are not according to order in specs (p. 49)
    public enum BetriebsPhaseHK
    {
        Tagbetrieb = 0,
        Nachtbetrieb = 1,
        Handbetrieb = 2,
        Kaminkehrerbetrieb = 3,
        Frostschutz = 4,
        Unter_Freigabetemperatur = 5,
        Aus = 6
    }
}
