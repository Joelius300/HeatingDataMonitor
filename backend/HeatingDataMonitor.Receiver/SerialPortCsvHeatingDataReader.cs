using System.IO.Ports;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace HeatingDataMonitor.Receiver;

// You'll find this class littered with comments, logs, exceptions and just-to-be-sure checks.
// This is because working with serial ports is an absolute joy and never poses any challenges :)
// It's easily the most challenging part of this whole system thus far while also being the most
// important core component, so I want to make sure everything works and can be understood later on.
// On top of that, https://github.com/dotnet/runtime/issues/62554 made it very difficult to test this class with
// an actual serial port. And yes, testing with a mocked serial port would be great wouldn't it? Unfortunately,
// successfully mocking SerialPort is in an of itself extremely hard and even if you manage to do it, you just wont
// be able to simulate the weird, borderline inexplicable issues you sometimes run into with serial ports.
internal class SerialPortCsvHeatingDataReader : ICsvHeatingDataReader, IAsyncEnumerable<string>
{
    private readonly SerialPortOptions _serialPortOptions;
    private readonly ILogger<SerialPortCsvHeatingDataReader> _logger;

    public SerialPortCsvHeatingDataReader(IOptions<SerialPortOptions> serialPortOptions,
        ILogger<SerialPortCsvHeatingDataReader> logger)
    {
        _serialPortOptions = serialPortOptions.Value;
        _logger = logger;
    }

    public IAsyncEnumerable<string> ReadCsvLines() => this;

    IAsyncEnumerator<string> IAsyncEnumerable<string>.GetAsyncEnumerator(CancellationToken cancellationToken) =>
        new SerialPortLineEnumerator(_serialPortOptions, _logger, cancellationToken);

    /* The enumerator uses a queue, which is populated from an event of the serial port.
     * On MoveNextAsync, one item is dequeued and put into Current.
     * The serial port will never stop providing data, therefore the enumeration is infinite unless cancelled.
     * Cancellation can happen with break or with WithCancellation(CancellationToken).
     * Disposal is handled automatically when using await foreach.
     */
    private class SerialPortLineEnumerator : IAsyncEnumerator<string>, IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly ILogger<SerialPortCsvHeatingDataReader> _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly Channel<string> _queue;
        private string? _unfinishedLine;
        private bool _disposed;

        public string Current { get; private set; } = null!;

        // ReSharper disable once ContextualLoggerProblem
        public SerialPortLineEnumerator(SerialPortOptions serialPortOptions, ILogger<SerialPortCsvHeatingDataReader> logger, CancellationToken cancellationToken)
        {
            _logger = logger;
            _cancellationToken = cancellationToken;
            // Using a bounded queue with 100 slots to avoid infinite memory growth if something goes wrong.
            // If everything works correctly, there will only be one item in the queue at a time because it will
            // be dequeued, parsed and saved to the db before the next item arrives (ca. 6 seconds later).
            // If for whatever reason the parsing and db-insertion take longer than that, the queue can contain more
            // than 1 item. It'd then have 10 min (6s * 100 slots) time to catch up aka speeding up again
            // before the queue is full and an exception is thrown (manually by us).
            _queue = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait // makes TryWrite return false for a full queue
            });

            // we can open the port this early because this class is only instantiated once the enumeration
            // starts (in the lowered code from an await foreach).
            _serialPort = SetupAndOpenSerialPort(serialPortOptions);
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            // Minor (correctness) optimization, in case token is cancelled during the very small time frame outside of WaitToReadAsync
            if (_cancellationToken.IsCancellationRequested)
                return false;

            bool hasMoreData;
            try
            {
                hasMoreData = await _queue.Reader.WaitToReadAsync(_cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation token from WithCancellation has signaled to cancel so the enumeration ends here.
                return false;
            }

            if (!hasMoreData)
            {
                if (!_disposed)
                    throw new InvalidOperationException("Okay something has gone VERY wrong.");

                throw new ObjectDisposedException($"Enumerator of call {nameof(ReadCsvLines)}",
                    "MoveNextAsync was called after queue was completed, " +
                    "which can only happen when the enumerator is disposed.");
            }

            if (!_queue.Reader.TryRead(out string? current)) // we should always get true here but just in case..
                throw new InvalidOperationException("Couldn't read from queue, even though we waited for it.");

            Current = current;

            return true;
        }

        private void ProcessSerialPortData(object sender, SerialDataReceivedEventArgs e)
        {
            string newData = _serialPort.ReadExisting();
            string allData = _unfinishedLine + newData;
            string[] separateLines = allData.Split(_serialPort.NewLine);
            if (separateLines.Length == 1) // no new data, append to unfinished line
            {
                _unfinishedLine = allData;
            }
            else
            {
                // enqueue all lines except for the last one, which becomes our new unfinished line.
                // usually there will only be one full line.
                for (int i = 0; i < separateLines.Length - 1; i++)
                {
                    if (!_queue.Writer.TryWrite(separateLines[i]))
                    {
                        if (_disposed) // we obviously can't write to a completed queue but that's not an error, just return
                            return;

                        // throwing in this context cannot be handled by anyone, so just log
                        _logger.LogError("Couldn't write to the queue (probably because it's full which means, the parsing and db-insertion took too long)");
                        Dispose();
                    }
                }

                _unfinishedLine = separateLines[^1];
            }
        }

        private SerialPort SetupAndOpenSerialPort(SerialPortOptions options)
        {
            SerialPort port = CreateSerialPort(options);
            port.Open();
            port.DataReceived += ProcessSerialPortData;

            return port;
        }

        private static SerialPort CreateSerialPort(SerialPortOptions options)
        {
            string portName = options.PortName;
            if (string.IsNullOrWhiteSpace(portName))
                throw new InvalidOperationException("The specified serial port name is invalid.");

            if (!SerialPort.GetPortNames().Contains(portName))
                throw new InvalidOperationException($"The specified serial port name '{portName}' was not found.");

            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(options.Encoding);
            }
            catch (ArgumentException e) when (e.ParamName == "name")
            {
                throw new InvalidOperationException($"The specified encoding '{options.Encoding}' is invalid or unsupported.");
            }

            return new SerialPort
            {
                PortName = portName,
                BaudRate = options.BaudRate,
                DataBits = options.DataBits,
                Parity = options.Parity,
                Handshake = options.Handshake,
                StopBits = options.StopBits,
                Encoding = encoding,
                DiscardNull = true,
                NewLine = options.NewLine
            };
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _queue.Writer.Complete(); // nothing will ever be written to the queue again!
            _serialPort.DataReceived -= ProcessSerialPortData;
            _serialPort.Dispose(); // .Close() is equal to .Dispose()
        }

        public ValueTask DisposeAsync()
        {
            Dispose();

            return default;
        }
    }
}
