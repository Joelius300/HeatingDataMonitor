using NodaTime;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IdentifierTypo

namespace HeatingDataMonitor.Models;

// This class contains a subset of properties of HeatingData.
// It allows typesafe, high performance partial data fetching from the db
// at the cost of redundancy and questionable data access (this is basically a DTO on the DAL).
// I wanted to avoid fetching everything and mapping later because of performance and avoid
// using a class like as base for HeatingData because of transparency. The redundancy is also
// manageable because the database seldom changes as the heating unit never changes.
public sealed class TemperatureChartData
{
    public Instant ReceivedTime { get; set; }
    public float Kessel { get; set; }
    public float Abgas { get; set; }
    public float Puffer_Oben { get; set; }
    public float Puffer_Unten { get; set; }
    public float Boiler_1 { get; set; }
}
