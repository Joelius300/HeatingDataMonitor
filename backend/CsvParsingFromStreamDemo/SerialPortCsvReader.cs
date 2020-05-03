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
        private bool _disposed = false;

        public event EventHandler<T> DataReceived;

        public SerialPortCsvReader(ISerialPort port, CsvConfiguration csvConfig, Encoding encoding, int bufferThreshold = DefaultBufferTheshold)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _bufferThreshold = bufferThreshold;
            _buffer = new MemoryStream(_bufferThreshold / 4); // there should only be 2 resizes at most
            _bufferReader = new StreamReader(_buffer, encoding ?? throw new ArgumentNullException(nameof(encoding)));
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
            long newDataStartPos = ReadSerialData();
            ProcessBufferedData(newDataStartPos);
        }

        private long ReadSerialData()
        {
            int availableBytes = _port.BytesToRead;
            if (availableBytes == 0)
                return _buffer.Length;

            if (availableBytes > _bufferThreshold)
                throw new InvalidOperationException($"The package received by the serial port is larger than the " +
                                                    $"specified buffer threshold ({_bufferThreshold} bytes) and can't be processed.");

            if (_buffer.Length + availableBytes > _bufferThreshold)
            {
                long expectedSize = _buffer.Length + availableBytes;

                // This won't deallocate the memory but it will prevent further allocations.
                _buffer.Position = 0;
                _buffer.SetLength(0);

                Console.WriteLine($"Buffer size would exeed threshold ({expectedSize} > {_bufferThreshold}). Buffer was reset.");
            }

            byte[] data = new byte[availableBytes];
            _port.Read(data, 0, data.Length);

            long currentLength = _buffer.Length;
            _buffer.Position = currentLength;
            _buffer.Write(data, 0, data.Length);

            return currentLength;
        }

        private void ProcessBufferedData(long fromPosition)
        {
            _buffer.Position = fromPosition; // don't feed it any data it's already seen

            // The CsvReader will copy all of our data into it's internal buffer on the first call to Read().
            // Then it will go through that internal buffer on sequential calls to Read().
            // This means that the _buffer.Position will always be = _buffer.Length after the first call to Read()
            // no matter the position of the CsvReader within the internal buffer (that would be _csvReader.Context.BufferPosition).
            // The problem is that the internal buffer get's cleared even when we might not be done with it.
            // Since the first Read() will move _buffer.Position to the end, we can't just update the last read position
            // because that might skip data.

            // It might be the best solution to investigate CsvHelper and create a special CsvParser which continues with the
            // last line in case it wasn't finished (EOF was reched).
            // This would probably mean overriding CsvParser.ReadLine but that's not enough.
            // Since the reading is done record by record and every record is read field by field, we need to customize almost
            // everything down the chain. The serial port spits out chunks and they might be in the middle of a field so when
            // the parser reads that, it will either fail or just take that incomplete part as field which is of course not what
            // we want. However, the reading is done in chunks as well (from the stream) so it should in theory be able to handle
            // that already. Again, more investigation is needed.

            // When it's time to give up, there's still the possibility to read the data from the port as string (will use the
            // encoding). Then we have a buffer for incomplete records and a buffer that the CsvReader works on (just like now).
            // Everytime we get data we combine it with the incomplete-records-buffer. Then loop through each line
            // (probably with a StringReader) and add each line (incl. line break) to the actual buffer as bytes. Any data that's
            // not complete (doesn't end with a line-break), will be put in the incomplete-records-buffer for the next run.
            // This will be a lot worse for performance but in our case that doesn't matter.

            // Based on the idea above, it might be possible to do it with two buffers while avoiding getting strings from the port.
            // We could read from the port into an internal buffer. Then go through that buffer and only add data that's terminated
            // by a newline (incl. said newline). We'll still need to decode the bytes once before passing it to the CsvReader
            // (otherwise we won't know where the line-breaks are) so performance will be negatively impacted. If this is better
            // than the above solution I don't know. These are just some ideas but doing it directly with CsvReader would be best.
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
