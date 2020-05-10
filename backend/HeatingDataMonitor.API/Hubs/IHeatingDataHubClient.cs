using HeatingDataMonitor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API.Hubs
{
    public interface IHeatingDataHubClient
    {
        Task ReceiveHeatingData(HeatingData data);
    }
}
