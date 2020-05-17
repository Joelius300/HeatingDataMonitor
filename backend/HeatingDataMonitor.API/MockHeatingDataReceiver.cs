using HeatingDataMonitor.Model;
using HeatingDataMonitor.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API
{
    public class MockHeatingDataReceiver : BackgroundService, IHeatingDataReceiver
    {
        private readonly ILogger<MockHeatingDataReceiver> _logger;
        private readonly Random _rng;

        public event EventHandler<HeatingData> DataReceived;
        public HeatingData Current { get; private set; }

        public MockHeatingDataReceiver(ILogger<MockHeatingDataReceiver> logger)
        {
            _logger = logger;
            _rng = new Random();
        }

        private HeatingData GetData()
        {
            return new HeatingData
            {
                SPS_Zeit = DateTime.Now,
                ReceivedTime_UTC = DateTime.UtcNow,
                Kessel = (float)Math.Round(_rng.NextDouble() * 60 + 20, 2),
                CO2_Ist = (float)Math.Round(_rng.NextDouble() * 30 + 10)
            };
        }

        protected virtual void OnDataReceived(HeatingData data)
        {
            Current = data;
            DataReceived?.Invoke(this, data);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                HeatingData data = GetData();
                OnDataReceived(data);
                try
                {
                    await Task.Delay(_rng.Next(5, 8) * 1000, stoppingToken);
                }
                catch (TaskCanceledException e)
                {
                    _logger.LogDebug(e.Message);
                }
            }
        }
    }
}
