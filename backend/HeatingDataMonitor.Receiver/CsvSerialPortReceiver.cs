using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks.Dataflow;
using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.Receiver;

public sealed class CsvSerialPortReceiver : IHeatingDataReceiver, IDisposable
{
    private readonly SerialPortOptions _options;
    private readonly CsvConfiguration _csvConfig;
    private readonly ILogger<CsvSerialPortReceiver> _logger;
    private readonly IClock _clock;
    private readonly SerialPort _serialPort;
    private readonly string[] _newLineSplitArray;

    public CsvSerialPortReceiver(IOptions<SerialPortOptions> options, ILogger<CsvSerialPortReceiver> logger, IClock clock)
    {
        _options = options.Value;
        _logger = logger;
        _clock = clock;

        _csvConfig = CreateCsvOptions();
        _serialPort = CreateSerialPort();
        _newLineSplitArray = new[] {_serialPort.NewLine};
    }

    public IAsyncEnumerable<HeatingData> StreamHeatingData(CancellationToken cancellationToken)
    {
        try
        {
            BufferBlock<string> lineQueue = StartReadingFromSerialPort(cancellationToken);
            return ParseLineQueue(lineQueue, cancellationToken);
        }
        finally
        {
            CloseSerialPort();
        }
    }

    private BufferBlock<string> StartReadingFromSerialPort(CancellationToken cancellationToken)
    {
        BufferBlock<string> queue = new();


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

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            Delimiter = _options.Delimiter,
            IgnoreBlankLines = true,
            Encoding = encoding,
            HasHeaderRecord = false
        };

        return config;
    }

    private SerialPort CreateSerialPort()
    {
        string portName = _options.PortName;
        if (string.IsNullOrWhiteSpace(portName))
            throw new InvalidOperationException("The specified serial port name is invalid.");

        if (!SerialPort.GetPortNames().Contains(portName))
            throw new InvalidOperationException($"The specified serial port name '{portName}' was not found.");

        return new SerialPort
        {
            PortName = portName,
            BaudRate = _options.BaudRate,
            DataBits = _options.DataBits,
            Parity = _options.Parity,
            Handshake = _options.Handshake,
            StopBits = _options.StopBits,
            Encoding = _csvConfig.Encoding,
            DiscardNull = true,
            NewLine = _options.NewLine
        };
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        // TODO
    }

    public void Dispose() => Dispose(true);
}
