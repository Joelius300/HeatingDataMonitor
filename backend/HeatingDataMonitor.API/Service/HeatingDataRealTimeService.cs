using System.Diagnostics;
using HeatingDataMonitor.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver.Shared;

namespace HeatingDataMonitor.API.Service;

/* When this service starts, it starts listening for real-time connections.
 * When the first real-time connection started, start streaming to clients.
 * When the last real-time connection ended, stop streaming to clients.
 * When the service stops, stop listening, stop streaming and wait for task to finish unless forced shutdown happens.
 */
internal sealed class HeatingDataRealTimeService : BackgroundService
{
    private readonly ILogger<HeatingDataRealTimeService> _logger;
    private readonly IHubContext<HeatingDataHub, IHeatingDataHubClient> _hubContext;
    private readonly IHeatingDataReceiver _heatingDataReceiver;
    private readonly IRealTimeConnectionManager _connectionManager;
    private readonly SemaphoreSlim _firstUserConnectedSemaphore;
    private CancellationTokenSource? _lastUserDisconnectedCts;

    public HeatingDataRealTimeService(ILogger<HeatingDataRealTimeService> logger,
        IHubContext<HeatingDataHub, IHeatingDataHubClient> hubContext, IHeatingDataReceiver heatingDataReceiver,
        IRealTimeConnectionManager connectionManager)
    {
        _logger = logger;
        _hubContext = hubContext;
        _heatingDataReceiver = heatingDataReceiver;
        _connectionManager = connectionManager;

        // max 1 concurrent use and requires Release for first use
        _firstUserConnectedSemaphore = new SemaphoreSlim(0, 1);
    }

    public override Task StartAsync(CancellationToken startupCancellationToken)
    {
        _connectionManager.FirstUserConnected += FirstClientConnected;
        _connectionManager.LastUserDisconnected += LastClientDisconnected;

        // starts ExecuteAsync in the background
        return base.StartAsync(startupCancellationToken);
    }

    private void FirstClientConnected(object? sender, EventArgs e)
    {
        try
        {
            _firstUserConnectedSemaphore.Release(); // make WaitUntilFirstClientConnects return
        }
        catch (SemaphoreFullException)
        {
            // if this happens the error could be.
            // - error in connection manager that allowed this event to fire twice without the other one in between
            // - race condition: if this event was is called multiple times (maybe correctly so) while the ExecuteAsync
            //   loop was wrapping around to start waiting for a new connection again, this could happen as well.
            //   This case should be recoverable, as nothing is stopping the main loop from continuing (even though
            //   it might go on for longer than necessary if a stop event was issued but the cancellation wasn't ready
            //   yet so it was ignored. Check LastClientDisconnected as they are very closely related.
            _logger.LogWarning("Tried to start streaming real-time data to clients but it was already started");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken serviceStoppingToken)
    {
        while (!serviceStoppingToken.IsCancellationRequested)
        {
            await WaitUntilFirstClientConnects(serviceStoppingToken);

            // create and store token which can be canceled from the last user disconnected event.
            // we also link it with the serviceStoppingToken to ensure the application can gracefully shutdown
            // even if there are still clients connected.
            _lastUserDisconnectedCts = CancellationTokenSource.CreateLinkedTokenSource(serviceStoppingToken);
            try
            {
                // stream data until token is canceled (which will return, not throw).
                // all exceptions bubble up to the host which (by default) terminates the application gracefully.
                await StreamDataToClients(_lastUserDisconnectedCts.Token);
            }
            finally
            {
                // ensure that the token stored in _lastUserDisconnectedCts is always valid for cancellation or null.
                _lastUserDisconnectedCts.Dispose();
                _lastUserDisconnectedCts = null;
            }
        }
    }

    private Task WaitUntilFirstClientConnects(CancellationToken cancellationToken) =>
        _firstUserConnectedSemaphore.WaitAsync(cancellationToken);

    private async Task StreamDataToClients(CancellationToken lastClientDisconnectedToken)
    {
        await foreach (HeatingData heatingData in _heatingDataReceiver.StreamHeatingData(lastClientDisconnectedToken))
        {
            await _hubContext.Clients.All.OnDataPointReceived(heatingData);
            _logger.LogTrace("Sent data to all clients with timestamp: {Timestamp}", heatingData.ReceivedTime);
        }
    }

    private void LastClientDisconnected(object? sender, EventArgs e)
    {
        if (_lastUserDisconnectedCts is null)
        {
            // if the cts is null when this event happens, the following things are possible:
            // - error in connection manager allowed this event to be fired before start streaming event
            // - race condition where this event fired almost instantly after the other one fired which made
            //   WaitUntilFirstClientConnects return but before _lastUserDisconnectedCts was assigned.
            //   This is recoverable but could cause the loop to go on for longer than necessary because this
            //   event was swallowed instead of being buffered.
            _logger.LogWarning("Tried to stop streaming real-time data to clients but it seemingly never started");
            return;
        }

        _lastUserDisconnectedCts.Cancel();
    }

    public override Task StopAsync(CancellationToken notGracefulAnymoreCancellationToken)
    {
        _connectionManager.FirstUserConnected -= FirstClientConnected;
        _connectionManager.LastUserDisconnected -= LastClientDisconnected;

        // this will cancel the serviceStoppingToken and wait for ExecuteAsync to end gracefully before killing it if necessary
        return base.StopAsync(notGracefulAnymoreCancellationToken);
    }

    public override void Dispose()
    {
        Debug.Assert(_lastUserDisconnectedCts is null, "_lastUserDisconnectedCts is null"); // already done
        _firstUserConnectedSemaphore.Dispose();
        base.Dispose();
    }
}
