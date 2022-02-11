using HeatingDataMonitor.Models;

namespace HeatingDataMonitor.Receiver.Shared;

/// <summary>
/// A service to stream strongly typed heating data from some source in real time.
/// </summary>
public interface IHeatingDataReceiver
{
    /// <summary>
    /// Returns an infinite stream of strongly typed heating data objects as they arrive in real time.
    /// You may use the <paramref name="cancellationToken"/> parameter, <c>WithCancellation(CancellationToken)</c>
    /// or <c>break</c> within an <c>await foreach</c> to stop steaming/receiving data.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel data streaming.</param>
    IAsyncEnumerable<HeatingData> StreamHeatingData(CancellationToken cancellationToken);
}
