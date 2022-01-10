using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Data.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.Data.Service;

// This contains too much redundancy from SerialPortHeatingDataReceiver. Maybe base class.
public sealed class MockHeatingDataReceiver : BackgroundService, IHeatingDataReceiver
{
    private readonly SerialHeatingDataOptions _options;
    private readonly ILogger<MockHeatingDataReceiver> _logger;
    private readonly IClock _clock;
    private StreamReader _fileReader = null!;
    private CsvReader _csvReader = null!;

    public event EventHandler<HeatingData>? DataReceived;
    public HeatingData? Current { get; private set; }

    public MockHeatingDataReceiver(IOptions<SerialHeatingDataOptions> options, ILogger<MockHeatingDataReceiver> logger, IClock clock)
    {
        _options = options.Value;
        _logger = logger;
        _clock = clock;
    }

    private void OnDataReceived(HeatingData data)
    {
        Current = data;
        DataReceived?.Invoke(this, data);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.PortName))
            throw new InvalidOperationException("The specified file (port name) can't be null or whitespace.");

        if (!Path.GetExtension(_options.PortName).Equals(".csv", StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(_options.PortName))
        {
            _logger.LogError("The sample data file at '{PortName}' is not an existing csv file", _options.PortName);
            return;
        }

        // TODO Improve csvReader instantiation logic (refactor method)
        CsvConfiguration csvConfig = CreateCsvOptions();

        _fileReader = new StreamReader(_options.PortName);
        _csvReader = new CsvReader(_fileReader, csvConfig);
        _csvReader.Context.RegisterClassMap<HeatingDataCsvMap>();

        while (!stoppingToken.IsCancellationRequested &&
               await _csvReader.ReadAsync())
        {
            HeatingData? record = null;
            try
            {
                record = _csvReader.GetRecord<HeatingData>();
            }
            catch (CsvHelperException e)
            {
                // Todo optimize logging arguments
                StringBuilder sb = new();
                sb.AppendLine($"Couldn't parse record to {nameof(HeatingData)}.");
                if (e.InnerException != null)
                {
                    sb.AppendLine($"Reason: {e.InnerException.Message}");
                }
                sb.AppendLine($"Record: {e.Context.Parser.RawRecord}");
                _logger.LogError(sb.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception while reading data - Aborting");
                break;
            }

            if (record != null)
            {
                record.ReceivedTime = _clock.GetCurrentInstant();
                OnDataReceived(record);
            }

            try
            {
                await Task.Delay(3000, stoppingToken);
            }
            catch (TaskCanceledException e)
            {
                _logger.LogDebug(e.Message);
                break;
            }
        }
    }

    private CsvConfiguration CreateCsvOptions()
    {
        if (string.IsNullOrEmpty(_options.Delimiter))
            throw new InvalidOperationException("The specified delimiter is invalid.");

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            Delimiter = _options.Delimiter,
            IgnoreBlankLines = true,
            HasHeaderRecord = false
        };

        return config;
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        _fileReader.Dispose();
        _csvReader.Dispose();

        // No need to dispose base as it would only cancel the service which
        // must already be done, otherwise this instance wouldn't get disposed.
    }

    public override void Dispose()
    {
        Dispose(true);
    }
}
