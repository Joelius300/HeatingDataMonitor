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
using DataHandler.Enums;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DataHandler.Services
{
    public class SerialDataService : DataService
    {
        private readonly SerialPort port;
        private readonly ILogger<SerialDataService> _logger;

        public SerialDataService(DataStorage dataStorage, IOptions<HeatingMonitorOptions> options, ILogger<SerialDataService> logger) : base(dataStorage, logger)
        {
            port = CreateSerialPort(options.Value.SerialPortName, options.Value.ExpectedReadIntervalInSeconds);
            _logger = logger;
        }

        private SerialPort CreateSerialPort(string portName, int expectedReadInterval) {
            return new SerialPort()
            {
                PortName = portName,                    // COM1 (Win), /dev/ttyS0 (raspi)
                BaudRate = 9600,                        // def from specs (heizung-sps)
                DataBits = 8,                           // def from specs (heizung-sps)
                Parity = Parity.None,                   // def from specs (heizung-sps)
                Handshake = Handshake.None,             // def from specs (heizung-sps)
                StopBits = StopBits.One,                // def from specs (heizung-sps)
                Encoding = Encoding.ASCII,              // def from specs (heizung-sps)
                DiscardNull = true,                     // we don't need that
                ReadTimeout = expectedReadInterval * 2000, // give enough time
                NewLine = "\r\n"                        // define newline used by sps
            };
        }

        private Data GetData()
        {
            string serialData;
            try
            {
                _logger.LogInformation("Now awaiting data: ");
                serialData = port.ReadLine();
            }
            catch (TimeoutException e)
            {
                _logger.LogWarning("Timeout: ");
                _logger.LogWarning(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogWarning("Canceled: ");
                _logger.LogWarning(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning("InvalidOperation: ");
                _logger.LogWarning(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message);

                throw new NoDataReceivedException(e);
            }

            Data newData = Data.FromSerialData(serialData);
            if (newData == null)
            {
                _logger.LogWarning("Received invalid Data: ");
                _logger.LogWarning(serialData);

                throw new FaultyDataReceivedException(serialData);
            }

            _logger.LogInformation("Received valid Data: ");
            _logger.LogInformation(serialData);

            return newData;
        }

        protected override async Task<Data> GetNewData(CancellationToken cancellationToken)
        {
            // make async call
            return await Task.Run(() => GetData());
        }

        protected override Task BeforeLoopStart()
        {
            port.Open();
            return Task.CompletedTask;
        }

        protected override async Task CleanupOnApplicationShutdown()
        {
            port.Close();           // this would raise an OperationCanceledException if the port is still reading
            await Task.Delay(250);  // give the loop one last chance to end (gracefully)
        }

        public override void Dispose()
        {
            port.Dispose();
            base.Dispose();
        }
    }
}
