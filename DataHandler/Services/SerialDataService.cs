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

namespace DataHandler.Services
{
    public class SerialDataService : DataService
    {
        private readonly SerialPort port;

        public SerialDataService(DataStorage dataStorage, Config config) : base(dataStorage)
        {
            port = CreateSerialPort(config);
        }

        private SerialPort CreateSerialPort(Config config) {
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
                ReadTimeout = config.ExpectedReadInterval * 2000, // give enough time
                NewLine = "\r\n"                        // define newline used by sps
            };
        }

        private Data GetData()
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

                throw new NoDataReceivedException(e);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Canceled: ");
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("InvalidOperation: ");
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }

            Data newData = Data.Convert(serialData);
            if (newData == null)
            {
                Console.WriteLine("Received invalid Data: ");
                Console.WriteLine(serialData);

                throw new FaultyDataReceivedException(serialData);
            }

            Console.WriteLine("Received valid Data: ");
            Console.WriteLine(serialData);

            return newData;
        }

        protected override async Task<Data> GetNewData()
        {
            return await Task.Run(() => GetData());
        }

        protected override Task Start()
        {
            port.Open();
            return Task.CompletedTask;
        }

        protected override async Task Stop()
        {
            port.Close();
            await Task.Delay(300);  // let the loop end
        }

        public override void Dispose()
        {
            port.Dispose();
            base.Dispose();
        }
    }
}
