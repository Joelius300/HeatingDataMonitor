using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Database.Read;

namespace HeatingDataMonitor.API.Hubs;

public sealed class HeatingDataHub : Hub<IHeatingDataHubClient>
{
    private readonly IHeatingDataRepository _heatingDataRepository;

    public HeatingDataHub(IHeatingDataRepository heatingDataRepository)
    {
        _heatingDataRepository = heatingDataRepository;
    }

    public Task<HeatingData?> GetCurrentHeatingData() => _heatingDataRepository.FetchLatestAsync();
}
