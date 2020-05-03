using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;

namespace CsvParsingFromStreamDemo
{
    public class SerialPortCsvReader<T> : IDisposable
    {
        private const int DefaultBufferTheshold = 4096;

        private readonly int _bufferThreshold;
        private readonly ISerialPort _port;
        private readonly CsvReader _csvReader;
        private readonly MemoryStream _buffer;
        private readonly StreamReader _bufferReader;
        private readonly AutoResetEvent _syncEvent;
        private bool _disposed = false;
        private long _readIndex; // the read index conveys up to where the csvReader has buffered our data

        public event EventHandler<T> DataReceived;

        public SerialPortCsvReader(ISerialPort port, CsvConfiguration csvConfig, Encoding encoding, int bufferThreshold = DefaultBufferTheshold)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _bufferThreshold = bufferThreshold;
            _buffer = new MemoryStream();
            _bufferReader = new StreamReader(_buffer, encoding ?? throw new ArgumentNullException(nameof(encoding)));
            _csvReader = new CsvReader(_bufferReader, csvConfig ?? throw new ArgumentNullException(nameof(csvConfig)));
            _syncEvent = new AutoResetEvent(true);
        }

        public void Start()
        {
            _port.Open();
            _port.DataReceived += DataReceivedHandler;
        }

        public void Stop()
        {
            _port.DataReceived -= DataReceivedHandler;
            _port.Close();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_syncEvent.WaitOne(1))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Was still processing, waiting for the next event.");
                return;
            }

            ReadSerialData();
            ProcessBufferedData();
        }

        private void ReadSerialData()
        {
            if (_buffer.Length >= _bufferThreshold)
            {
                // reset buffer so it won't grow further. This won't deallocate the memory.
                long curSize = _buffer.Length;
                ClearBuffer();
                Console.WriteLine($"Buffer size was above threshold ({curSize} >= {_bufferThreshold}). Shortened to {_buffer.Length}.");
            }

            byte[] data = new byte[_port.BytesToRead];
            _port.Read(data, 0, data.Length);
            _buffer.Position = _buffer.Length;
            _buffer.Write(data, 0, data.Length);
        }

        private void ClearBuffer()
        {
            long unreadBytes = _buffer.Length - _readIndex;
            if (unreadBytes > 0)
            {
                byte[] unreadData = new byte[unreadBytes];
                _buffer.Position = _readIndex;
                _buffer.Read(unreadData, 0, unreadData.Length);
                _buffer.Position = 0;
                _buffer.SetLength(unreadData.Length);
                _buffer.Write(unreadData, 0, unreadData.Length);
            }
            else
            {
                _buffer.Position = 0;
                _buffer.SetLength(0);
            }

            _readIndex = 0;
        }

        private void ProcessBufferedData()
        {
            _buffer.Position = _readIndex; // don't feed it any data it's already seen

            // The CsvReader will copy all of our data into it's internal buffer on the first call to Read().
            // Then it will go through that internal buffer on sequential calls to Read().
            // This means that the _buffer.Position will always be = _buffer.Length after the first call to Read()
            // no matter the position of the CsvReader within the internal buffer (that would be _csvReader.Context.BufferPosition).
            while (_csvReader.Read())
            {
                bool parsed = false;
                T record = default;
                try
                {
                    record = _csvReader.GetRecord<T>();
                    parsed = true;
                }
                catch (CsvHelperException e)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Couldn't parse record to {typeof(T).Name}.");
                    if (e.InnerException != null)
                    {
                        sb.AppendLine($"Reason: {e.InnerException.Message}");
                    }
                    sb.AppendLine($"Record: {e.ReadingContext.RawRecord}");
                    Console.WriteLine(sb.ToString());
                }

                if (parsed)
                {
                    OnDataReceived(record);
                }
            }

            _readIndex = _buffer.Position;

            _syncEvent.Set();
        }

        protected virtual void OnDataReceived(T data)
        {
            DataReceived?.Invoke(this, data);
        }

        // Also disposes underlying serial port
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                Stop();

                if (disposing)
                {
                    _port.Dispose();
                    _csvReader.Dispose();
                    _buffer.Dispose();
                    _bufferReader.Dispose();
                    _syncEvent.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
