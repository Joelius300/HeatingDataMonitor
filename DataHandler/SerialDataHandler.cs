using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using DataHandler.Exceptions;
using System.CodeDom.Compiler;

namespace DataHandler
{
    public class SerialDataHandler : IDataHandler
    {
        private readonly SerialPort port;
        public Data CurrentData { get; private set; }
        public bool NoDataReceived { get; private set; }
        public string FaultyDataReceived { get; private set; }
        public int ExpectedReadInterval { get; private set; }
        

        private CancellationTokenSource cts;
        private Task loopTask;

        public event Action Changed;

        public SerialDataHandler(Config config)
        {
            ExpectedReadInterval = config.ExpectedReadInterval;
            port = CreateSerialPort(config);
            Start();
        }

        protected virtual SerialPort CreateSerialPort(Config config) {
            return new SerialPort()
            {
                PortName = config.SerialPortName,       // COM1 (Win), /dev/ttyS0 (raspian)
                BaudRate = 9600,                        // def from specs (heizung-sps)
                DataBits = 8,                           // def from specs (heizung-sps)
                Parity = Parity.None,                   // def from specs (heizung-sps)
                Handshake = Handshake.None,             // def from specs (heizung-sps)
                StopBits = StopBits.One,                // def from specs (heizung-sps)
                Encoding = Encoding.ASCII,              // def from specs (heizung-sps)
                DiscardNull = true,                     // we don't need that
                ReadTimeout = ExpectedReadInterval * 2, // give enough time
                NewLine = "\r\n"                        // define newline used by sps
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
                Console.WriteLine("Now awaiting data: ");
                serialData = port.ReadLine();
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout: ");
                Console.WriteLine(e.Message);

                NoDataReceived = true;
                return;
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Canceled: ");
                Console.WriteLine(e.Message);

                NoDataReceived = true;
                return;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("InvalidOperation: ");
                Console.WriteLine(e.Message);

                NoDataReceived = true;
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                NoDataReceived = true;
                return;
            }

            Data newData = Data.Convert(serialData);
            if (newData == null)
            {
                Console.WriteLine("Received invalid Data: ");
                Console.WriteLine(serialData);

                FaultyDataReceived = serialData;
                NoDataReceived = true;
                return;
            }

            Console.WriteLine("Received valid Data: ");
            Console.WriteLine(serialData);

            CurrentData = newData;
            NoDataReceived = false;
        }

        private void Start()
        {
            Console.WriteLine("Opening serial port " + port.PortName);
            try
            {
                port.Open();
                cts = new CancellationTokenSource();
                loopTask = LoopAsync(cts.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when starting the serial-port reader: ");
                Console.WriteLine(e.Message);
            }
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
