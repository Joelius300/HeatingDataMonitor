using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Database.Views;
using NodaTime;

namespace HeatingDataMonitor.Database;

public interface IHeatingDataRepository
{
    // TODO Add mention of notification
    /// <summary>
    /// Inserts a record into the database.
    /// </summary>
    /// <param name="heatingData">The .NET instance of this record.</param>
    public Task InsertRecordAsync(HeatingData heatingData);

    // TODO IAsyncEnumerable once Dapper supports it
    /// <summary>
    /// Fetches all records within a certain time from the database.
    /// </summary>
    /// <param name="from">Start of timespan.</param>
    /// <param name="to">End of timespan.</param>
    public Task<IEnumerable<HeatingData>> FetchAsync(Instant from, Instant to);

    /// <summary>
    /// Fetches a certain sub-part of all records within a certain time from the database.
    /// This sub-part contains the relevant columns for displaying a temperature chart on the UI.
    /// </summary>
    /// <param name="from">Start of timespan.</param>
    /// <param name="to">End of timespan.</param>
    public Task<IEnumerable<TemperatureChartData>> FetchMainTemperaturesAsync(Instant from, Instant to);
}
