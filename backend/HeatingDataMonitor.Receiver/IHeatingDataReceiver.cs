using HeatingDataMonitor.Models;
using Microsoft.Extensions.Hosting;

namespace HeatingDataMonitor.Receiver;

public interface IHeatingDataReceiver : IHostedService
{
    event EventHandler<HeatingData> DataReceived;
    HeatingData? Current { get; }
}
