using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;

namespace DataHandler
{
    public class SerialDataHandler : IDataHandler
    {
        private readonly SerialPort port;
        public Data CurrentData { get; private set; }
        public bool NoDataReceived { get; private set; }
        public string FaultyDataReceived { get; private set; }
        public int ExpectedReadInterval { get; private set; }

        private readonly Config config;
        private CancellationTokenSource cts;
        private Task loopTask;

        public event Action Changed;

        public SerialDataHandler()
        {
            config = Config.Deserialize();
            ExpectedReadInterval = config.ExpectedReadTimeout;
            port = CreateSerialPort();
            Start();
        }

        protected virtual SerialPort CreateSerialPort() {
            return new SerialPort()
            {
                PortName = config.SerialPortName,   // COM1 (Win), /dev/ttyS0 (raspian)
                BaudRate = 9600,                    // def from specs (heizung)
                DataBits = 8,                       // def from specs (heizung)
                Parity = Parity.None,               // def from specs (heizung)
                Handshake = Handshake.None,         // def from specs (heizung)
                StopBits = StopBits.One,            // def from specs (heizung)
                Encoding = Encoding.ASCII,          // def from specs (heizung)
                DiscardNull = true,
                ReadTimeout = ExpectedReadInterval * 3
                //ReadTimeout = -1
            };
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
            string serialData;
            try
            {
                serialData = port.ReadLine();
                Console.WriteLine($"Incoming serial-data: {serialData}");
            }
            catch (TimeoutException)
            {
                NoDataReceived = true;
                return;
            }
            catch (OperationCanceledException)
            {
                NoDataReceived = true;
                return;
            }
            catch (InvalidOperationException)
            { 
                NoDataReceived = true;
                return;
            }

            Data newData = Data.Convert(serialData);
            if (newData == null)
            {
                FaultyDataReceived = serialData;
                NoDataReceived = true;
                return;
            }

            CurrentData = newData;
            NoDataReceived = false;
        }

        private void Start()
        {
            port.Open();
            cts = new CancellationTokenSource();
            loopTask = LoopAsync(cts.Token);
        }

        private void Stop()
        {
            cts.Cancel();
            loopTask.Wait(3000);
            port.Close();
        }

        public void Dispose()
        {
            Stop();
            cts?.Dispose();
            port.Dispose();
        }
    }
}
