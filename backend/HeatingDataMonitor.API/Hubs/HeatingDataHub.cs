using HeatingDataMonitor.API.Service;
using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Database.Read;

namespace HeatingDataMonitor.API.Hubs;

public sealed class HeatingDataHub : Hub<IHeatingDataHubClient>
{
    private readonly IHeatingDataRepository _heatingDataRepository;
    private readonly IRealTimeConnectionManager _connectionManager;

    public HeatingDataHub(IHeatingDataRepository heatingDataRepository, IRealTimeConnectionManager connectionManager)
    {
        _heatingDataRepository = heatingDataRepository;
        _connectionManager = connectionManager;
    }

    public Task<HeatingData?> GetCurrentHeatingData() => _heatingDataRepository.FetchLatestAsync();

    // Note comments in RealTimeConnectionManager; this works fine for our purposes but won't for multiple servers etc.
    public override Task OnConnectedAsync()
    {
        _connectionManager.ClientConnected(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionManager.ClientDisconnected(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
