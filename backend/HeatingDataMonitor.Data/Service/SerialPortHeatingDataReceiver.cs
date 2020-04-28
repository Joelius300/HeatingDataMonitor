using HeatingDataMonitor.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Service
{
    public class SerialPortHeatingDataReceiver : IHeatingDataReceiver
    {
        public HeatingData Current => throw new NotImplementedException();

        public event EventHandler<HeatingData> DataReceived;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private SerialPort CreateSerialPort(string portName, int expectedReadInterval) =>
            new SerialPort()
            {
                // TODO: Take from some sort of option instance (think IOption<SerialPortOptions>)

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
}
