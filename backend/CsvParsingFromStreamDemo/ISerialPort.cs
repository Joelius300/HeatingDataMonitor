using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace CsvParsingFromStreamDemo
{
    public interface ISerialPort : IDisposable
    {
        event SerialDataReceivedEventHandler DataReceived;
        void Open();
        void Close();
        int BytesToRead { get; }
        int Read(byte[] buffer, int offset, int count);
    }
}
