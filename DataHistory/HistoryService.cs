using DataHandler;
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
        protected HeatingDataContext Context { get; }

        private Data lastDataAdded;

        public HistoryService(Config config, DataStorage dataStorage, HeatingDataContext context)
        {
            Config = config;
            DataStorage = dataStorage;
            Context = context;
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

                if (toAdd == null) return;  // shouldn't happen
                if (toAdd.DatumZeit <= (lastDataAdded?.DatumZeit ?? DateTime.MinValue)) return;

                Context.Data.Add(toAdd);
                lastDataAdded = toAdd;
                await Context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
