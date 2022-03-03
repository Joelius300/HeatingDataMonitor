using HeatingDataMonitor.Database.Models;

namespace HeatingDataMonitor.API.Hubs;

public interface IHeatingDataHubClient
{
    Task OnDataPointReceived(HeatingData data);
}
