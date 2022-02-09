using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Models;
using NodaTime;

namespace HeatingDataMonitor.Receiver;

public sealed class CsvHeatingDataReceiver : IHeatingDataReceiver, IDisposable
{
    private readonly ICsvHeatingDataReader _csvHeatingDataReader;
    private readonly ILogger<CsvHeatingDataReceiver> _logger;
    private readonly IClock _clock;
    private readonly CsvConfiguration _csvConfig;

    public CsvHeatingDataReceiver(ICsvHeatingDataReader csvHeatingDataReader, ILogger<CsvHeatingDataReceiver> logger, IClock clock)
    {
        _csvHeatingDataReader = csvHeatingDataReader;
        _logger = logger;
        _clock = clock;

        _csvConfig = CreateCsvOptions();
    }

    public async IAsyncEnumerable<HeatingData> StreamHeatingData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using MemoryStream buffer = new();
        await using StreamWriter bufferWriter = new(buffer);
        using StreamReader bufferReader = new(buffer);
        using CsvReader csvReader = new(bufferReader, _csvConfig);

        await foreach (string line in _csvHeatingDataReader.ReadCsvLines().WithCancellation(cancellationToken))
        {
            // make sure to reset the buffer to avoid unnecessary memory growth
            buffer.Position = 0;
            buffer.SetLength(0);

            await bufferWriter.WriteLineAsync(line); // writes csv data + new line
            await bufferWriter.FlushAsync();

            if (!await csvReader.ReadAsync()) // since there's always one full line in the buffer, this shouldn't fail
                throw new InvalidOperationException("Couldn't read csv line from buffer.");

            HeatingData record;
            try
            {
                record = csvReader.GetRecord<HeatingData>();
            }
            catch (CsvHelperException e)
            {
                // If the data is incomplete, the record obviously can't be parsed.
                // Currently there's no safeguard to only return full lines, because the DataReader
                // starts reading as soon as the connection to the serial port is opened which
                // usually is in the middle of a record. We could handle the first record differently
                // but that would just be a workaround instead of correctly implementing the safe-guard.

                // tl;dr it's normal for the first record to be parsed incorrectly, which will log a warning.
                _logger.LogWarning(e, "Couldn't parse record: {Record}", e.Context.Parser.RawRecord);
                continue; // skip this record
            }

            record.ReceivedTime = _clock.GetCurrentInstant();
            yield return record;
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
            HasHeaderRecord = false,
            CountBytes = false,
            MemberTypes = MemberTypes.Properties
        };

        return config;
    }
}
