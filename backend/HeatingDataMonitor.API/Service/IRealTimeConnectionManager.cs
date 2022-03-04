namespace HeatingDataMonitor.API.Service;

/// <summary>
/// A service to listen for newly added or removed real-time clients.
/// </summary>
public interface IRealTimeConnectionManager
{
    int ConnectedCount { get; }
    event EventHandler FirstClientConnected;
    event EventHandler LastClientDisconnected;

    void ClientConnected(string connectionId);
    void ClientDisconnected(string connectionId);
}
