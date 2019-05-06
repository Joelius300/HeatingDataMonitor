using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataHandler
{
    public class MockDataHandler : IDataHandler
    {
        public Data CurrentData { get; private set; }

        public bool NoDataReceived { get; private set; }

        public int ExpectedReadInterval { get; private set; }

        public event Action Changed;

        private readonly Config config;
        private CancellationTokenSource cts;
        private Task loopTask;

        public MockDataHandler()
        {
            config = Config.Deserialize();
            ExpectedReadInterval = config.ExpectedReadInterval;
            Start();
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            await Task.Run(() => {
                while (!ct.IsCancellationRequested)
                {
                    ReadToProp(); // expected to block for a bit

                    // notify everyone that there is some new data
                    var temp = Changed;
                    temp?.Invoke();
                }
            });
        }

        protected virtual void ReadToProp()
        {
            CurrentData = Data.GetRandomData();
            Thread.Sleep(ExpectedReadInterval);
        }

        private void Start()
        {
            cts = new CancellationTokenSource();
            loopTask = LoopAsync(cts.Token);
        }

        private void Stop()
        {
            cts.Cancel();
            loopTask.Wait(3000);
        }

        public void Dispose()
        {
            Stop();
            cts?.Dispose();
        }
    }
}
