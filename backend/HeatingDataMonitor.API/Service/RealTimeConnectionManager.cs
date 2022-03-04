namespace HeatingDataMonitor.API.Service;

// SignalR was created with large applications (meaning multiple servers,
// load balancing, etc.) in mind which is why there's not built-in way
// to do this as it needs to be synced over an unknown infrastructure.
// In our case though, we just have one server and this implementation will suffice.
// Tested this manually as it's pretty simple (bad excuse I know).
public sealed class RealTimeConnectionManager : IRealTimeConnectionManager
{
    private readonly ILogger<RealTimeConnectionManager> _logger;
    private readonly HashSet<string> _connectionIds = new();

    public event EventHandler? FirstClientConnected;
    public event EventHandler? LastClientDisconnected;

    // ReSharper disable once InconsistentlySynchronizedField
    public int ConnectedCount => _connectionIds.Count;

    public RealTimeConnectionManager(ILogger<RealTimeConnectionManager> logger) => _logger = logger;

    public void ClientConnected(string connectionId)
    {
        bool firstClientConnected;
        lock (_connectionIds)
        {
            bool didAdd = _connectionIds.Add(connectionId);
            firstClientConnected = didAdd && ConnectedCount == 1;
        }

        if (firstClientConnected)
        {
            OnFirstClientConnected();
        }
    }

    private void OnFirstClientConnected()
    {
        _logger.LogDebug("First real-time client connected");
        FirstClientConnected?.Invoke(this, EventArgs.Empty);
    }

    public void ClientDisconnected(string connectionId)
    {
        bool lastClientDisconnected;
        lock (_connectionIds)
        {
            bool didRemove = _connectionIds.Remove(connectionId);
            lastClientDisconnected = didRemove && ConnectedCount == 0;
        }

        if (lastClientDisconnected)
        {
            OnLastClientDisconnected();
        }
    }

    private void OnLastClientDisconnected()
    {
        _logger.LogDebug("Last real-time client disconnected");
        LastClientDisconnected?.Invoke(this, EventArgs.Empty);
    }
}
