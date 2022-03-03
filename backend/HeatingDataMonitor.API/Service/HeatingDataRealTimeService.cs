using HeatingDataMonitor.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver.Shared;

namespace HeatingDataMonitor.API.Service;

/* Reiterating general idea because I thought this would be simple
 * but I'm struggling. You can probably take a lot of inspiration from BackgroundService.
 * When this service starts, it just starts listening for real-time connections.
 * When the first real-time connection started, start streaming to clients.
 * When the last real-time connection ended, stop streaming to clients.
 * When the service ends, stop listening, stop streaming and wait for task to finish unless forced shutdown happens.
 */
internal sealed class HeatingDataRealTimeService : IHostedService, IAsyncDisposable
{
    private readonly ILogger<HeatingDataRealTimeService> _logger;
    private readonly IHubContext<HeatingDataHub, IHeatingDataHubClient> _hubContext;
    private readonly IHeatingDataReceiver _heatingDataReceiver;
    private readonly IRealTimeConnectionManager _connectionManager;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private Task? _streamingTask;
    private CancellationTokenSource? _cts;

    public HeatingDataRealTimeService(ILogger<HeatingDataRealTimeService> logger,
        IHubContext<HeatingDataHub, IHeatingDataHubClient> hubContext, IHeatingDataReceiver heatingDataReceiver,
        IRealTimeConnectionManager connectionManager, IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _hubContext = hubContext;
        _heatingDataReceiver = heatingDataReceiver;
        _connectionManager = connectionManager;
        _applicationLifetime = applicationLifetime;
    }

    private async Task StreamDataToClients(CancellationToken stoppingToken)
    {
        await foreach (HeatingData heatingData in _heatingDataReceiver.StreamHeatingData(stoppingToken))
        {
            await _hubContext.Clients.All.OnDataPointReceived(heatingData);
            _logger.LogTrace("Sent data to all clients with timestamp: {Timestamp}", heatingData.ReceivedTime);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _connectionManager.FirstUserConnected += StartStreaming;
        _connectionManager.LastUserDisconnected += StopStreaming;
    }

    private async void StartStreaming(object? sender, EventArgs e) => await StartStreaming();
    private async Task StartStreaming()
    {
        if (_streamingTask is not null && _streamingTask.)
        {
            _logger.LogWarning("Real-time data streaming was started twice");
            return;
        }

        _cts = new CancellationTokenSource();
        _streamingTask = StreamDataToClients(_cts.Token);
    }

    private void StopStreaming(object? sender, EventArgs e) => StopStreaming();
    private void StopStreaming()
    {
        if (_cts is null)
        {
            _logger.LogError("Real-time data streaming was stopped without being started");
            return;
        }

        _cts.Cancel();
        _cts.Dispose();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {

    }
}
