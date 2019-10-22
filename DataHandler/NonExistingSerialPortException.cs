using System;
using System.Runtime.Serialization;

namespace DataHandler
{
    public class NonExistingSerialPortException : Exception
    {
        public string PortName { get; }

        public NonExistingSerialPortException(string port) : 
            base($"Der SerialPort '{port}' wurde nicht gefunden. Versuchen Sie 'COM*' für Windows und '/dev/tty*' für Linux ('/dev/ttyS0' für Raspberry Pi).")
        {
            PortName = port;
        }
    }
}