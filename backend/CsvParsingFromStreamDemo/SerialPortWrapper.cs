using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace CsvParsingFromStreamDemo
{
    public class SerialPortWrapper : ISerialPort
    {
        private readonly SerialPort _port;

        public SerialPortWrapper(SerialPort serialPort) => _port = serialPort ?? throw new ArgumentNullException(nameof(serialPort));

        public int BytesToRead => _port.BytesToRead;

        public event SerialDataReceivedEventHandler DataReceived
        {
            add => _port.DataReceived += value;
            remove => _port.DataReceived -= value;
        }

        public void Close() => _port.Close();

        public void Dispose() => _port.Dispose();

        public void Open() => _port.Open();

        public int Read(byte[] buffer, int offset, int count) => _port.Read(buffer, offset, count);
    }
}
