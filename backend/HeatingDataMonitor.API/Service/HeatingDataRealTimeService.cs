using HeatingDataMonitor.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver.Shared;

namespace HeatingDataMonitor.API.Service;

public class HeatingDataRealTimeService : BackgroundService
{
    private readonly ILogger<HeatingDataRealTimeService> _logger;
    private readonly IHubContext<HeatingDataHub, IHeatingDataHubClient> _hubContext;
    private readonly IHeatingDataReceiver _heatingDataReceiver;

    public HeatingDataRealTimeService(ILogger<HeatingDataRealTimeService> logger,
        IHubContext<HeatingDataHub, IHeatingDataHubClient> hubContext,
        IHeatingDataReceiver heatingDataReceiver)
    {
        _logger = logger;
        _hubContext = hubContext;
        _heatingDataReceiver = heatingDataReceiver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO This is just for testing it in the beginning. Instead this should only
        // start sending real-time data when at least one client requests it.
        await foreach (HeatingData heatingData in _heatingDataReceiver.StreamHeatingData(stoppingToken))
        {
            await _hubContext.Clients.All.OnDataPointReceived(heatingData);
            _logger.LogTrace("Sent data to all clients with timestamp: {Timestamp}", heatingData.ReceivedTime);
        }
    }
}
