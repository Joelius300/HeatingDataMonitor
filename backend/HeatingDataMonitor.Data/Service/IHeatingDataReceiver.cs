using HeatingDataMonitor.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Service
{
    public interface IHeatingDataReceiver : IHostedService
    {
        event EventHandler<HeatingData> DataReceived;
        HeatingData Current { get; }
    }
}
