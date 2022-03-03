namespace HeatingDataMonitor.API.Service;

// SignalR was created with large applications (meaning multiple servers,
// load balancing, etc.) in mind which is why there's not built-in way
// to do this as it needs to be synced over an unknown infrastructure.
// In our case though, we just have one server and this implementation will suffice.
// Tested this manually as it's pretty simple (bad excuse I know).
public class RealTimeConnectionManager : IRealTimeConnectionManager
{
    private readonly HashSet<string> _connectionIds = new();

    public event EventHandler? FirstUserConnected;
    public event EventHandler? LastUserDisconnected;

    // ReSharper disable once InconsistentlySynchronizedField
    public int ConnectedCount => _connectionIds.Count;

    public void UserConnected(string connectionId)
    {
        bool firstUserConnected;
        lock (_connectionIds)
        {
            bool didAdd = _connectionIds.Add(connectionId);
            firstUserConnected = didAdd && ConnectedCount == 1;
        }

        if (firstUserConnected)
        {
            FirstUserConnected?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UserDisconnected(string connectionId)
    {
        bool lastUserDisconnected;
        lock (_connectionIds)
        {
            bool didRemove = _connectionIds.Remove(connectionId);
            lastUserDisconnected = didRemove && ConnectedCount == 0;
        }

        if (lastUserDisconnected)
        {
            LastUserDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }
}
