using System.Diagnostics;
using HeatingDataMonitor.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver.Shared;

namespace HeatingDataMonitor.API.Service;

/* When the service starts, start listening for real-time connections.
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
    private readonly SemaphoreSlim _firstClientConnectedSemaphore;
    private CancellationTokenSource? _lastClientDisconnectedCts;

    public HeatingDataRealTimeService(ILogger<HeatingDataRealTimeService> logger,
        IHubContext<HeatingDataHub, IHeatingDataHubClient> hubContext, IHeatingDataReceiver heatingDataReceiver,
        IRealTimeConnectionManager connectionManager)
    {
        _logger = logger;
        _hubContext = hubContext;
        _heatingDataReceiver = heatingDataReceiver;
        _connectionManager = connectionManager;

        // max 1 concurrent use and requires Release for first use
        _firstClientConnectedSemaphore = new SemaphoreSlim(0, 1);
    }

    public override Task StartAsync(CancellationToken startupCancellationToken)
    {
        _connectionManager.FirstClientConnected += FirstClientConnected;
        _connectionManager.LastClientDisconnected += LastClientDisconnected;

        // starts ExecuteAsync in the background
        return base.StartAsync(startupCancellationToken);
    }

    private void FirstClientConnected(object? sender, EventArgs e)
    {
        try
        {
            _firstClientConnectedSemaphore.Release(); // make WaitUntilFirstClientConnects return
        }
        catch (SemaphoreFullException)
        {
            // if this happens the error could be.
            // - error in connection manager that allowed this event to fire twice without the disconnect one in between
            // - ~race condition~ I don't think the race condition I had in mind (same as disconnect) could trigger this anymore
            //   thanks to the guard in LastClientDisconnected but it's very possible I missed some other race condition.
            _logger.LogWarning("Tried to start streaming real-time data to clients but it was already started");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken serviceStoppingToken)
    {
        while (!serviceStoppingToken.IsCancellationRequested)
        {
            await WaitUntilFirstClientConnects(serviceStoppingToken);

            _logger.LogInformation("Start streaming real-time data to SignalR clients");

            // create and store token which can be canceled from the last client disconnected event.
            // we also link it with the serviceStoppingToken to ensure the application can gracefully shutdown
            // even if there are still clients connected.
            _lastClientDisconnectedCts = CancellationTokenSource.CreateLinkedTokenSource(serviceStoppingToken);
            try
            {
                // stream data until token is canceled (which will return, not throw).
                // all exceptions bubble up to the host which (by default) terminates the application gracefully.
                await StreamDataToClients(_lastClientDisconnectedCts.Token);
            }
            finally
            {
                // ensure that the token stored in _lastClientDisconnectedCts is always valid for cancellation or null.
                _lastClientDisconnectedCts.Dispose();
                _lastClientDisconnectedCts = null;
            }

            _logger.LogInformation("Stopped streaming real-time data to SignalR clients");
        }
    }

    private Task WaitUntilFirstClientConnects(CancellationToken cancellationToken) =>
        _firstClientConnectedSemaphore.WaitAsync(cancellationToken);

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
        // ensure semaphore is set to 0
        if (_firstClientConnectedSemaphore.Wait(0))
        {
            _logger.LogWarning("Race condition in real-time streaming service - clients connected and disconnected too fast");
            // Semaphore could still be 1 when clients are connecting and disconnecting faster than
            // the ExecuteAsync loop can handle. Scenario:
            // - first client connects -> semaphore is set to 1 and instantly consumed to start streaming
            // - last client disconnects -> cancellation of streaming loop starts with disposal of cts etc.
            // - new client connects before cancellation is done -> semaphore increases count to 1 again
            // - client quickly disconnects again before cancellation is done ->
            //   cannot cancel because it's not started yet and semaphore is still on 1 so the streaming would start again
            // To avoid this, we quickly try to take away that 1 on the semaphore which will prevent the streaming from
            // starting until a new client connects. However, this is a theoretical race condition and I'm trying to make
            // sure issues like this are considered and caught early to avoid mind-boggling debugging session later on.
        }

        if (_lastClientDisconnectedCts is null)
        {
            // if the cts is null when this event happens, the following things are possible:
            // - error in connection manager allowed this event to be fired before start streaming event
            // - race condition where this event fired almost instantly after the other one fired which made
            //   WaitUntilFirstClientConnects return but before _lastClientDisconnectedCts was assigned.
            //   This is recoverable and thanks to the guard above, the loop shouldn't go on for longer than necessary.
            _logger.LogWarning("Tried to stop streaming real-time data to clients but it seemingly never started");
            return;
        }

        _lastClientDisconnectedCts.Cancel();
    }

    public override Task StopAsync(CancellationToken notGracefulAnymoreCancellationToken)
    {
        _connectionManager.FirstClientConnected -= FirstClientConnected;
        _connectionManager.LastClientDisconnected -= LastClientDisconnected;

        // this will cancel the serviceStoppingToken and wait for ExecuteAsync to end gracefully before killing it if necessary
        return base.StopAsync(notGracefulAnymoreCancellationToken);
    }

    public override void Dispose()
    {
        Debug.Assert(_lastClientDisconnectedCts is null, "_lastClientDisconnectedCts is null"); // already done
        _firstClientConnectedSemaphore.Dispose();
        base.Dispose();
    }
}
