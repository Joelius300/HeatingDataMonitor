﻿using HeatingDataMonitor.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Data.Model;
using HeatingDataMonitor.Data.Service;

namespace HeatingDataMonitor.API.Service;

public class HeatingDataRealTimeService : IHostedService
{
    private readonly ILogger<HeatingDataRealTimeService> _logger;
    private readonly IHubContext<HeatingDataHub, IHeatingDataHubClient> _hubContext;
    private readonly EventHandler<HeatingData> _receivedHandler;
    private readonly IHeatingDataReceiver _heatingDataReceiver;

    public HeatingDataRealTimeService(ILogger<HeatingDataRealTimeService> logger,
        IHubContext<HeatingDataHub, IHeatingDataHubClient> hubContext,
        IHeatingDataReceiver heatingDataReceiver)
    {
        _logger = logger;
        _hubContext = hubContext;
        _heatingDataReceiver = heatingDataReceiver;
        _receivedHandler = SendReceivedData;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _heatingDataReceiver.DataReceived += _receivedHandler;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _heatingDataReceiver.DataReceived -= _receivedHandler;

        return Task.CompletedTask;
    }

    private async void SendReceivedData(object? sender, HeatingData newData)
    {
        await _hubContext.Clients.All.OnDataPointReceived(newData);
        _logger.LogDebug("Sent data to all clients with timestamp: {Timestamp}", newData.ReceivedTime);
    }
}
