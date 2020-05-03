using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace CsvParsingFromStreamDemo
{
    public class SerialPortCsvReader<T> : IDisposable
    {
        private readonly SerialPort _port;
        private readonly CsvReader _csvReader;
        private readonly MemoryStream _buffer;
        private readonly StreamReader _bufferReader;
        private readonly AutoResetEvent _readingEvent;
        // If these threads both completely occupy a logical processing unit, we might want to
        // try and switch to async. Since there's a lot of waiting involved, the os might sort it out but idk.
        // The motivation behind that is that this will be used in asp.net core where
        // async is the default for background processing. Also this will run on a
        // raspberry pi which doesn't have a lot of cores.
        // Or we could do everything in the same thread
        private readonly Thread _readingThread;
        private readonly Thread _processingThread;
        private readonly CancellationTokenSource _cts;
        private bool _disposed = false;
        private int _readIndex;

        public event EventHandler<T> DataReceived;

        public SerialPortCsvReader(SerialPort port, CsvReader csvReader, Encoding encoding)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _csvReader = csvReader ?? throw new ArgumentNullException(nameof(csvReader));
            
            _buffer = new MemoryStream();
            _bufferReader = new StreamReader(_buffer, encoding ?? throw new ArgumentNullException(nameof(encoding)));
            _readingEvent = new AutoResetEvent(true);
            _readingThread = new Thread(obj => ReadFromPort((CancellationToken)obj));
            _processingThread = new Thread(obj => ProcessBufferedData((CancellationToken)obj));
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            _readingThread.Start(_cts.Token);
            _processingThread.Start(_cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private void ReadFromPort(CancellationToken cancellationToken)
        {
            _port.Open();
            _port.DataReceived += DataReceived;

            cancellationToken.WaitHandle.WaitOne();
            _port.DataReceived -= DataReceived;

            void DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                if (!_readingEvent.WaitOne(100))
                    return;

                byte[] data = new byte[_port.BytesToRead];
                _port.Read(data, 0, data.Length);
                _buffer.Position = _buffer.Length;
                _buffer.Write(data, 0, data.Length);

                //Console.WriteLine($"Buffer pos after direct write: {_bufferStream.Position}");
                //Console.WriteLine($"Serial data received ({data.Length} bytes)");
            }
        }

        private void ProcessBufferedData(CancellationToken cancellationToken)
        {

        }

        protected virtual void OnDataReceived(T data)
        {
            DataReceived?.Invoke(this, data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
