using System;
using System.Collections.Generic;
using System.Text;
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

        protected override async Task<Data> GetNewData()
        {
            await Task.Delay(TimeSpan.FromSeconds(timeout));

            return Data.GetRandomData();
        }
    }
}
