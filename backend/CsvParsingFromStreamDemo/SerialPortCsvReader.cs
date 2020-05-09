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
        private readonly ISerialPort _port;
        private readonly CsvReader _csvReader;
        private readonly MemoryStream _buffer;
        private readonly StreamReader _bufferReader;
        private readonly StreamWriter _bufferWriter;
        private string _unfinishedLine;
        private bool _disposed = false;

        public event EventHandler<T> DataReceived;

        public SerialPortCsvReader(ISerialPort port, CsvConfiguration csvConfig)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _buffer = new MemoryStream();
            _bufferReader = new StreamReader(_buffer, _port.Encoding);
            _bufferWriter = new StreamWriter(_buffer, _port.Encoding);
            _csvReader = new CsvReader(_bufferReader, csvConfig ?? throw new ArgumentNullException(nameof(csvConfig)));
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
            // let's forget the fancy stuff for now. Who cares if we encode and decode this data
            // 5 more times than necessary if the data only appears every few seconds.
            // Performance is definitely not a driving enough factor to go crazy with this.
            // It would be very nice to have a custom CsvParser that continues where it left
            // off and acts conservative meaning it only finishes a field when it encounters
            // a field separator and it only finishes a line when it encounters a new line.
            // EOF simply means 'the rest will come soon'. It's definitely possible to implement
            // and would give a huge boost to performance along with simply being a cool thing
            // to implement. For now, let's just work with a much simpler solution at the
            // (completely irrelevant) cost of performance and the (a bit more relevant) cost
            // of failure at implementing such a custom CsvParser (for now).
            // YAGNI is more important than I like to admit

            string currentData = _port.ReadExisting();
            string fullData = _unfinishedLine + currentData;
            string[] lines = fullData.Split(_port.NewLine);
            if (lines.Length == 1)
            {
                _unfinishedLine = fullData;
            }
            else
            {
                _buffer.Position = 0;
                _buffer.SetLength(0);
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    _bufferWriter.WriteLine(lines[i]);
                }
                _bufferWriter.Flush();

                _unfinishedLine = lines[^1];

                ProcessBufferedData();
            }
        }

        private void ProcessBufferedData()
        {
            _buffer.Position = 0;

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
                }

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
