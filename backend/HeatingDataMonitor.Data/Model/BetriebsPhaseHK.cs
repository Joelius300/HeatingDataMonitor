// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
namespace HeatingDataMonitor.Data.Model;

// int values are not according to order in specs (p. 49)
public enum BetriebsPhaseHK
{
    // going through the data I realized it goes up to 7?
    // I still don't know the corresponding values tho..
    Tagbetrieb = 0,
    Nachtbetrieb = 1,
    Handbetrieb = 2,
    Kaminkehrerbetrieb = 3,
    Frostschutz = 4,
    Unter_Freigabetemperatur = 5,
    Aus = 6
}
