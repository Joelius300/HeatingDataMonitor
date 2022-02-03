using System.IO.Ports;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace HeatingDataMonitor.Receiver;

/* new Idea..
 * From outside:
 * - Create Class with SerialPort config
 * - Call method which returns IAsyncEnumerable and loop over it
 * - Everything is setup when the call is made and everything is disposed once iteration is finished
 * - You could iterate multiple times on the same instance by calling the method multiple times
 * - It's not threadsafe, no concurrent iteration because SerialPort just doesn't work that way
 * From inside:
 * - The actual public facing class should only contain everything required to setup and start a new iteration
 * - It could either have a method which returns IAsyncEnumerable which contains all the necessary stuff as local vars
 * - Or it could return a new class which contains all the necessary stuff as members. Because we're also working with events here, this might be the better option.
 * - The inner class
 */
public class SerialPortCsvHeatingDataReader : ICsvHeatingDataReader
{
    private readonly SerialPortOptions _serialPortOptions;

    public SerialPortCsvHeatingDataReader(IOptions<SerialPortOptions> serialPortOptions) =>
        _serialPortOptions = serialPortOptions.Value;

    public IAsyncEnumerable<string> ReadCsvLines(CancellationToken cancellationToken) =>
        new ReadingEnumerable(_serialPortOptions, cancellationToken);

    private class ReadingEnumerable : IAsyncEnumerable<string>, IDisposable, IAsyncDisposable
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Channel<string> _queue;
        private readonly SerialPort _serialPort;
        private string? _unfinishedLine;
        private bool _startedReading;

        public ReadingEnumerable(SerialPortOptions serialPortOptions, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
            });
            _cancellationToken.Register(() => _queue.Writer.TryComplete());
            _serialPort = SetupSerialPort(serialPortOptions);
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
                throw new InvalidOperationException(
                    $"The specified encoding '{options.Encoding}' is invalid or unsupported.");
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

        private SerialPort SetupSerialPort(SerialPortOptions options)
        {
            SerialPort port = CreateSerialPort(options);
            port.DataReceived += ProcessSerialPortData;
            port.Open();

            return port;
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
                // enqueue all lines except for the last one, which becomes our new unfinished line
                for (int i = 0; i < separateLines.Length - 1; i++)
                {
                    _queue.Writer.TryWrite(separateLines[i]);
                }

                _unfinishedLine = separateLines[^1];
            }
        }

        public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (_startedReading)
                throw new InvalidOperationException("Cannot read more than once on this enumeration container.");

            _startedReading = true;
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationToken);

            return _queue.Reader.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        }

        public void Dispose() => throw new NotImplementedException();

        public async ValueTask DisposeAsync() => throw new NotImplementedException();

    }
}
