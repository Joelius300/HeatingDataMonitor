using HeatingDataMonitor.Models;
using Microsoft.Extensions.Hosting;

namespace HeatingDataMonitor.Receiver;

public interface IHeatingDataReceiver
{
    IAsyncEnumerable<HeatingData> StreamHeatingData(CancellationToken cancellationToken);
}
