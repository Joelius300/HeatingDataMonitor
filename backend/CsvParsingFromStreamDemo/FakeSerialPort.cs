using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace CsvParsingFromStreamDemo
{
    public class FakeSerialPort : ISerialPort
    {
        private readonly MemoryStream _data;
        private long _readPos;
        internal FakeSerialPort(MemoryStream stream) => _data = stream;
        public FakeSerialPort() : this(new MemoryStream())
        {
        }

        public void AddData(byte[] newSerialData)
        {
            _data.Position = _data.Length;
            _data.Write(newSerialData, 0, newSerialData.Length);
            DataReceived?.Invoke(this, null);
        }

        public int BytesToRead => (int)(_data.Length - _readPos);

        public event SerialDataReceivedEventHandler DataReceived;

        public void Close()
        {
            Console.WriteLine("Fake port closed");
        }

        public void Dispose()
        {
            _data.Dispose();
            Console.WriteLine("Fake port disposed");
        }

        public void Open()
        {
            Console.WriteLine("Fake port opened");
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            _data.Position = _readPos;
            int read = _data.Read(buffer, offset, count);
            _readPos = _data.Position;

            return read;
        }
    }
}
