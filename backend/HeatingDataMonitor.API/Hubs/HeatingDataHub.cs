using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API.Hubs
{
    public class HeatingDataHub : Hub<IHeatingDataHubClient>
    {
        // Currently the client can't do anything on the server
    }
}
