using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver;

namespace HeatingDataMonitor.API.Hubs;

public sealed class HeatingDataHub : Hub<IHeatingDataHubClient>
{
    private readonly IHeatingDataReceiver _heatingDataReceiver;

    public HeatingDataHub(IHeatingDataReceiver heatingDataReceiver)
    {
        _heatingDataReceiver = heatingDataReceiver;
    }

    public HeatingData? GetCurrentHeatingData() => _heatingDataReceiver.Current;
}
