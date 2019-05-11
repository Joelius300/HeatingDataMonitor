using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataHandler.Services
{
    public abstract class DataService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly DataStorage dataStorage;

        public DataService(DataStorage dataStorage) => this.dataStorage = dataStorage;

        private async Task LoopAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Data newData = null;
                try
                {
                    newData = await GetNewData();
                }
                catch (Exceptions.NoDataReceivedException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.InnerException.Message);
                }
                catch (Exceptions.FaultyDataReceivedException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.FaultyData);
                }

                if (newData == null) return;

                // update the current data
                dataStorage.CurrentData = newData;

                // signal that there is some new data
                dataStorage.OnNewDataReceived();
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Start();  // start configuration
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await Stop();   // forced stop
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return LoopAsync(stoppingToken);
        }

        protected abstract Task<Data> GetNewData();

        protected virtual Task Start() => Task.CompletedTask;
        protected virtual Task Stop() => Task.CompletedTask;
    }
}
