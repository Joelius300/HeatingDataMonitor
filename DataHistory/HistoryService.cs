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
        private readonly DataStorage dataStorage;
        private readonly Config config;

        private Data lastDataAdded;

        public HistoryService(Config config, DataStorage dataStorage)
        {
            this.config = config;
            this.dataStorage = dataStorage;

            try
            {
                HeatingDataContext.Instance = new HeatingDataContext(config.HistorySQLiteConnectionString);
                HeatingDataContext.Instance.Database.EnsureCreated();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error bei der Datenbank: {e.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(config.HistorySaveDelayInMinutes ?? 1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                Data toAdd = dataStorage.CurrentData;

                if (toAdd == null) continue;  // shouldn't happen because CurrentData should never be null
                if (toAdd.DatumZeit <= (lastDataAdded?.DatumZeit ?? DateTime.MinValue)) continue;   // twice the same data -> skip

                try
                {
                    HeatingDataContext.Instance.Data.Add(toAdd);
                    lastDataAdded = toAdd;
                    await HeatingDataContext.Instance.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error bei der Datenbank (hinzufügen): {e.Message}");
                    return; // something broke, abort
                }
            }
        }

        public override void Dispose()
        {
            HeatingDataContext.Instance.Dispose();
            base.Dispose();
        }
    }
}
