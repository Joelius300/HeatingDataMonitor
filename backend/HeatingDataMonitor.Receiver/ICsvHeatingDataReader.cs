namespace HeatingDataMonitor.Receiver;

/// <summary>
/// An interface for reading CSV-strings one line/record at a time.
/// </summary>
public interface ICsvHeatingDataReader
{
    IAsyncEnumerable<string> ReadCsvLines(CancellationToken cancellationToken);
}
