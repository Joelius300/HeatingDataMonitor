using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Model;
using HeatingDataMonitor.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Service
{
    // This contains too much redundancy from SerialPortHeatingDataReceiver. Maybe base class.
    public class MockHeatingDataReceiver : BackgroundService, IHeatingDataReceiver
    {
        private readonly SerialHeatingDataOptions _options;
        private readonly ILogger<MockHeatingDataReceiver> _logger;
        private StreamReader _fileReader;
        private CsvReader _csvReader;

        public event EventHandler<HeatingData> DataReceived;
        public HeatingData Current { get; private set; }

        public MockHeatingDataReceiver(IOptions<SerialHeatingDataOptions> options, ILogger<MockHeatingDataReceiver> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        protected virtual void OnDataReceived(HeatingData data)
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
                _logger.LogError($"The sample data file at '{_options.PortName}' is not an existing csv file.");
                return;
            }

            CsvConfiguration csvConfig = CreateCsvOptions();

            _fileReader = new StreamReader(_options.PortName);
            _csvReader = new CsvReader(_fileReader, csvConfig);

            while (!stoppingToken.IsCancellationRequested &&
                   _csvReader.Read())
            {
                HeatingData record = null;
                try
                {
                    record = _csvReader.GetRecord<HeatingData>();
                }
                catch (CsvHelperException e)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Couldn't parse record to {nameof(HeatingData)}.");
                    if (e.InnerException != null)
                    {
                        sb.AppendLine($"Reason: {e.InnerException.Message}");
                    }
                    sb.AppendLine($"Record: {e.ReadingContext.RawRecord}");
                    _logger.LogError(sb.ToString());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected exception while reading data. Aborting.");
                    break;
                }

                if (record != null)
                {
                    record.ReceivedTime = SystemClock.Instance.GetCurrentInstant();
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

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _options.Delimiter,
                IgnoreBlankLines = true,
                IgnoreQuotes = _options.ReadQuotesAsText,
                HasHeaderRecord = false
            };

            config.RegisterClassMap<HeatingDataCsvMap>();

            return config;
        }
    }
}
