using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataHandler.Services
{
    public sealed class MockDataService : DataService
    {
        private readonly int timeout;

        public MockDataService(DataStorage dataStorage, IOptions<HeatingMonitorOptions> config) : base(dataStorage)
        {
            timeout = config.Value.ExpectedReadIntervalInSeconds;
        }

        protected override async Task<Data> GetNewData(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeout), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            // don't generate random, get one-by-one from database
            return Data.GetRandomData();
        }
    }
}
