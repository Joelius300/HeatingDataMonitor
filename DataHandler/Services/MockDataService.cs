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

        public MockDataService(DataStorage dataStorage, Config config) : base(dataStorage)
        {
            timeout = config.ExpectedReadInterval;
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

            return Data.GetRandomData();
        }
    }
}
