using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.History;
using HeatingDataMonitor.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Service
{
    // todo internal
    public class SerialPortHeatingDataReceiver : IHeatingDataReceiver, IDisposable
    {
        private readonly SerialHeatingDataOptions _options;
        private readonly CsvConfiguration _csvConfig;
        private readonly ILogger<SerialPortHeatingDataReceiver> _logger;
        private readonly SerialPort _serialPort;
        private readonly Thread _readingThread;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _cancelToken;
        private CsvReader _csvReader;
        private bool _disposed = false;

        public HeatingData Current { get; private set; }
        public event EventHandler<HeatingData> DataReceived;

        public SerialPortHeatingDataReceiver(IOptions<SerialHeatingDataOptions> options, ILogger<SerialPortHeatingDataReceiver> logger)
        {
            _options = options.Value;
            _logger = logger;

            _cts = new CancellationTokenSource();
            _cancelToken = _cts.Token;
            _csvConfig = CreateCsvOptions();
            _serialPort = CreateSerialPort();
            _readingThread = new Thread(() => Read());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _serialPort.Open();
            _csvReader = new CsvReader(new StreamReader(_serialPort.BaseStream), _csvConfig);
            _readingThread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();

            return Task.CompletedTask;
        }

        private void Read()
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                HeatingData data = null;
                try
                {
                    /* # Problem at the moment
                     * When the read on the serial port times out, an exception is thrown
                     * and apparently the reader can't recover from that. It seems that writing
                     * "abc" then timing out and writing "def\r\n" will result in something along the
                     * lines of "\0\0\0def". Therefore we would need to set the timeout high enough for
                     * a whole line to be transferred but that means that we can't really shutdown the
                     * application gracefully as it's still reading when we want to close the port.
                     * Also, when a timout or error happens, we would need to discard all the
                     * buffered data to achieve a clean reset otherwise we have null bytes or left over
                     * line breaks in the actual data which will mess up the parsing.
                     * 
                     * # Possible approaches
                     * - Read out the data piece by piece into a buffer from a separate reading thread.
                     *   Then a different thread would go through that buffer, search for the line break
                     *   and parse up to there. The buffer would then be adjusted so only the data after
                     *   the line break is still in the buffer and the rest (already parsed) is discarded.
                     *   There should be some way of telling the parsing thread to look through the data
                     *   whenever the reading thread reads something. If we don't do this event-based, we
                     *   would need a polling mechanism which just tries to parse the existing data in a
                     *   set interval and skips when there is no data to parse but working with an event-
                     *   based pattern would be nicer.
                     *   For the parsing thread we would probably need a custom implementation of TextReader
                     *   if StreamReader doesn't work. This might be required because the CsvReader only
                     *   accepts a TextReader so there's no way to read from just a string or byte- or
                     *   char-array. Also, the CsvReader is designed for reading multiple lines but we only
                     *   ever want to read one line at a time since the serial port delivers data really
                     *   slowly.
                     * - Use ReadLine on the serial port and then parse somehow with a custom stream reader
                     *   thingy because we can't parse a string directly and creating a string reader for
                     *   every entry is really bad. If it's possible to reset the data in the string-reader
                     *   we should be able to use that. This again has the issue of graceful shutdown. If
                     *   no data is lost when there are multiple timeouts, we can just set a smaller timeout.
                     *   Otherwise we'll need to sequentially read all the data and manually search for the
                     *   line break very similar to option one. Optimally ReadLine should behave as follows.
                     *   ReadLine: "ab", timeout, "cd", timeout, "ef", timeout, "gh\r\n" needs to yield
                     *   "abcdefgh". This can only happen when everything gets reset correctly after a timeout
                     *   (that's ReadLine's job). Data-loss in between might result in faulty data or plain
                     *   parsing errors.
                     *   
                     *   As you can see by this and also all the comments in the SerialPort class, this is
                     *   quite a big thing and there are a lot of things to keep in mind when implementing this.
                     */

                    _csvReader.Read();
                    data = _csvReader.GetRecord<HeatingData>();
                }
                catch (Exception e) when (e is TimeoutException || 
                                          (e is ParserException ex && ex.InnerException is TimeoutException))
                {
                    _logger.LogDebug("Reading and parsing from serial port timed out.");
                }
                catch (ReaderException e)
                {
                    _logger.LogError(e, $"Error reading the data from the serial port:{Environment.NewLine}" +
                                        $"{e.InnerException?.Message}{Environment.NewLine}{e.ReadingContext.RawRecord}");
                }
                catch (ParserException e)
                {
                    _logger.LogError(e, $"Error parsing the data from the serial port:{Environment.NewLine}" +
                                        $"{e.InnerException?.Message}{Environment.NewLine}{e.ReadingContext.RawRecord}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected exception while reading data. Aborting.");
                    break;
                }

                if (data != null)
                {
                    OnDataReceived(data);
                }
            }
        }

        protected virtual void OnDataReceived(HeatingData data)
        {
            Current = data;
            DataReceived?.Invoke(this, data);
        }

        private CsvConfiguration CreateCsvOptions()
        {
            if (string.IsNullOrEmpty(_options.Delimiter))
                throw new InvalidOperationException("The specified delimiter is invalid.");

            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(_options.Encoding);
            }
            catch (ArgumentException e) when (e.ParamName == "name")
            {
                throw new InvalidOperationException($"The specified encoding '{_options.Encoding}' is invalid or unsupported.");
            }

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _options.Delimiter,
                NewLine = _options.NewLine,
                IgnoreBlankLines = true,
                IgnoreQuotes = _options.ReadQuotesAsText,
                Encoding = encoding,
                HasHeaderRecord = false
            };

            config.RegisterClassMap<HeatingDataCsvMap>();

            return config;
        }

        private SerialPort CreateSerialPort()
        {
            string portName = _options.PortName;
            if (string.IsNullOrWhiteSpace(portName))
                throw new InvalidOperationException("The specified serial port name is invalid.");

            if (!SerialPort.GetPortNames().Contains(portName))
                throw new InvalidOperationException($"The specified serial port name '{portName}' was not found.");

            return new SerialPort()
            {
                PortName = portName,
                BaudRate = _options.BaudRate,
                DataBits = _options.DataBits,
                Parity = _options.Parity,
                Handshake = _options.Handshake,
                StopBits = _options.StopBits,
                Encoding = _csvConfig.Encoding,
                DiscardNull = true,
                NewLine = _csvConfig.NewLineString,
                ReadTimeout = _options.ReadTimeoutMs // TODO think through | we need to somehow allow graceful shutdown
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts.Dispose();
                    _csvReader?.Dispose();
                    _serialPort.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
