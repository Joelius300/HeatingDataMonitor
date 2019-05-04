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

        private readonly Random rnd;

        public MockDataHandler()
        {
            rnd = new Random();
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
            CurrentData = GetRandomData();
            Thread.Sleep(6000);
        }

        private Data GetRandomData()
        {
            Data data = new Data
            {
                Datum = DateTime.UtcNow,
                Uhrzeit = DateTime.UtcNow,
                Kessel = float.Parse(rnd.NextDouble().ToString()),
                Ruecklauf = float.Parse(rnd.NextDouble().ToString()),
                Abgas = float.Parse(rnd.NextDouble().ToString()),
                Brennkammer = float.Parse(rnd.NextDouble().ToString()),
                CO2_Soll = float.Parse(rnd.NextDouble().ToString()),
                CO2_Ist = float.Parse(rnd.NextDouble().ToString()),
                Saugzug_Ist = float.Parse(rnd.NextDouble().ToString()),
                Puffer_Oben = float.Parse(rnd.NextDouble().ToString()),
                Puffer_Unten = float.Parse(rnd.NextDouble().ToString()),
                Platine = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_Kessel = rnd.Next(500),
                Aussen = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK1_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK1_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK1 = rnd.Next(500),
                Betriebsart_Fern_HK1 = rnd.Next(500),
                Verschiebung_Fern_HK1 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK1 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK2_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK2_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK2 = rnd.Next(500),
                Betriebsart_Fern_HK2 = rnd.Next(500),
                Verschiebung_Fern_HK2 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK2 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK3_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK3_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK3 = rnd.Next(500),
                Betriebsart_Fern_HK3 = rnd.Next(500),
                Verschiebung_Fern_HK3 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK3 = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK4_Ist = float.Parse(rnd.NextDouble().ToString()),
                Vorlauf_HK4_Soll = float.Parse(rnd.NextDouble().ToString()),
                Betriebsphase_HK4 = rnd.Next(500),
                Betriebsart_Fern_HK4 = rnd.Next(500),
                Verschiebung_Fern_HK4 = float.Parse(rnd.NextDouble().ToString()),
                Freigabekontakt_HK4 = float.Parse(rnd.NextDouble().ToString()),
                Boiler_1 = float.Parse(rnd.NextDouble().ToString()),
                Boiler_2 = float.Parse(rnd.NextDouble().ToString()),
                DI_0 = rnd.Next(500),
                DI_1 = rnd.Next(500),
                DI_2 = rnd.Next(500),
                DI_3 = rnd.Next(500),
                A_W_0 = rnd.Next(500),
                A_W_1 = rnd.Next(500),
                A_W_2 = rnd.Next(500),
                A_W_3 = rnd.Next(500),
                A_EA_0 = rnd.Next(500),
                A_EA_1 = rnd.Next(500),
                A_EA_2 = rnd.Next(500),
                A_EA_3 = rnd.Next(500),
                A_EA_4 = rnd.Next(500),
                A_PHASE_0 = rnd.Next(500),
                A_PHASE_1 = rnd.Next(500),
                A_PHASE_2 = rnd.Next(500),
                A_PHASE_3 = rnd.Next(500),
                A_PHASE_4 = rnd.Next(500)
            };
            data.CalcCustomProps();

            return data;
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
