using HeatingDataMonitor.Database.Models;

namespace HeatingDataMonitor.Database.Write;

/// <summary>
/// Write-only repository for adding records of heating data to a database.
/// </summary>
public interface IHeatingDataWriteRepository
{
    /// <summary>
    /// Inserts a record into the database. This also triggers a Postgres notification which can be used
    /// to stream newly inserted records.
    /// </summary>
    /// <param name="heatingData">The .NET instance of this record.</param>
    public Task InsertRecordAsync(HeatingData heatingData);
}
