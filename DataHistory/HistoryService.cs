using DataHandler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        protected HistoryServiceOptions Config { get; }
        protected IServiceScopeFactory ScopeFactory { get; }

        private Data lastDataAdded;

        public HistoryService(IOptions<HistoryServiceOptions> options, DataStorage dataStorage, IServiceScopeFactory scopeFactory)
        {
            Config = options.Value;
            DataStorage = dataStorage;
            ScopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(Config.SaveIntervalInMinutes), stoppingToken);
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
                    using IServiceScope scope = ScopeFactory.CreateScope();
                    IHistoryRepository repos = scope.ServiceProvider.GetRequiredService<IHistoryRepository>();
                    repos.Add(toAdd);
                    lastDataAdded = toAdd;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Couldn't save to DB: {e.Message}");
                }
            }
        }
    }
}
