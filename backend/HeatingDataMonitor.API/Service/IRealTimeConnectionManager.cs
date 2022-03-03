namespace HeatingDataMonitor.API.Service;

/// <summary>
/// A service to listen for newly added or removed real-time clients.
/// </summary>
public interface IRealTimeConnectionManager
{
    int ConnectedCount { get; }
    event EventHandler FirstUserConnected;
    event EventHandler LastUserDisconnected;

    void UserConnected(string connectionId);
    void UserDisconnected(string connectionId);
}
