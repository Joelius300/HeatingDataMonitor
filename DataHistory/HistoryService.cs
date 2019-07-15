using DataHandler;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataHistory
{
    public class HistoryService : Microsoft.Extensions.Hosting.BackgroundService
    {
        protected DataStorage DataStorage { get; }
        protected Config Config { get; }
        protected IServiceScopeFactory ScopeFactory { get; }

        private Data lastDataAdded;

        public HistoryService(Config config, DataStorage dataStorage, IServiceScopeFactory scopeFactory)
        {
            Config = config;
            DataStorage = dataStorage;
            ScopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(Config.HistorySaveDelayInMinutes ?? 1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                Data toAdd = DataStorage.CurrentData;

                if (toAdd == null) continue;  // shouldn't happen because CurrentData should never be null
                if (toAdd.DatumZeit <= lastDataAdded?.DatumZeit) continue;   // twice the same data -> skip

                try
                {
                    using (var scope = ScopeFactory.CreateScope())
                    {
                        IHistoryRepository repos = scope.ServiceProvider.GetRequiredService<IHistoryRepository>();
                        repos.Add(toAdd);
                        lastDataAdded = toAdd;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Couldn't save to DB: {e.Message}");
                }
            }
        }
    }
}
