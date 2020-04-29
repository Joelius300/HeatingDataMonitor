using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace HeatingDataMonitor.Service
{
    internal class SerialPortOptionsSetup : IConfigureOptions<SerialPortOptions>
    {
        public void Configure(SerialPortOptions options)
        {
            string[] availablePorts = SerialPort.GetPortNames();
            if (availablePorts.Length == 0)
                throw new InvalidOperationException("No available ports found on this machine.");

            Array.Sort(availablePorts);
            options.PortName = availablePorts[0];
        }
    }
}
