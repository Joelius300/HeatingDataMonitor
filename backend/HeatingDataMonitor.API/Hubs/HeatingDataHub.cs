using Microsoft.AspNetCore.SignalR;
using HeatingDataMonitor.Data.Model;
using HeatingDataMonitor.Data.Service;

namespace HeatingDataMonitor.API.Hubs;

public sealed class HeatingDataHub : Hub<IHeatingDataHubClient>
{
    private readonly IHeatingDataReceiver _heatingDataReceiver;
    //private readonly IServiceScopeFactory _scopeFactory;

    public HeatingDataHub(IHeatingDataReceiver heatingDataReceiver)
    {
        _heatingDataReceiver = heatingDataReceiver;
        //_scopeFactory = scopeFactory;
    }

    public HeatingData? GetCurrentHeatingData() => _heatingDataReceiver.Current;

    // Currently not needed but works as expected
    //public HeatingData GetLastArchivedHeatingData()
    //{
    //    using IServiceScope scope = _scopeFactory.CreateScope();
    //    HeatingDataDbContext context = scope.ServiceProvider.GetRequiredService<HeatingDataDbContext>();

    //    return context.HeatingData
    //                  .OrderBy(d => d.ReceivedTime)
    //                  .Last();
    //}
}
