namespace HeatingDataMonitor.Receiver;

/// <summary>
/// An interface for reading CSV-strings one line/record at a time.
/// </summary>
// The fact that it's CSV data is just by convention for this project. No implementation guarantees.
public interface ICsvHeatingDataReader
{
    /// <summary>
    /// Returns an infinite asynchronous enumerable which supports <c>WithCancellation(CancellationToken)</c>.
    /// Use <c>await foreach</c> together with <c>break</c> and <c>WithCancellation(CancellationToken)</c>
    /// to automatically handle cancellation and disposal. No enumerations returned from this method can
    /// be enumerated concurrently for the same serial port.
    /// </summary>
    IAsyncEnumerable<string> ReadCsvLines();
}
