using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DataHandler.Enums
{
    public enum BetriebsPhaseHK
    {
        Tagbetrieb = 0,
        Nachtbetrieb = 1,
        Handbetrieb = 2,
        Kaminkehrerbetrieb = 3,
        Frostschutz = 4,
        [Description("Unter Freigabetemperatur")]
        Unter_Freigabetemperatur = 5,
        Aus = 6
    }
}
