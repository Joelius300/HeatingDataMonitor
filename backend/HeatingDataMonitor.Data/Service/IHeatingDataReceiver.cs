using HeatingDataMonitor.Data.Model;
using Microsoft.Extensions.Hosting;

namespace HeatingDataMonitor.Data.Service;

public interface IHeatingDataReceiver : IHostedService
{
    event EventHandler<HeatingData> DataReceived;
    HeatingData? Current { get; }
}
