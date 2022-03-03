using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver.Shared;
using NodaTime;

namespace HeatingDataMonitor.Receiver;

internal sealed class CsvHeatingDataReceiver : IHeatingDataReceiver
{
    private const string NewLine = "\n";
    private static readonly CsvConfiguration s_csvConfig = CreateCsvOptions();
    private readonly ICsvHeatingDataReader _csvHeatingDataReader;
    private readonly ILogger<CsvHeatingDataReceiver> _logger;
    private readonly IClock _clock;

    public CsvHeatingDataReceiver(ICsvHeatingDataReader csvHeatingDataReader, ILogger<CsvHeatingDataReceiver> logger, IClock clock)
    {
        _csvHeatingDataReader = csvHeatingDataReader;
        _logger = logger;
        _clock = clock;
    }

    public async IAsyncEnumerable<HeatingData> StreamHeatingData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using MemoryStream buffer = new();
        await using StreamWriter bufferWriter = new(buffer) { NewLine = NewLine };
        using StreamReader bufferReader = new(buffer);
        using CsvReader csvReader = new(bufferReader, s_csvConfig);
        csvReader.Context.RegisterClassMap<HeatingDataCsvMap>();

        await foreach (string line in _csvHeatingDataReader.ReadCsvLines().WithCancellation(cancellationToken))
        {
            // make sure to reset the buffer to avoid unnecessary memory growth
            buffer.Position = 0;
            buffer.SetLength(0);

            await bufferWriter.WriteLineAsync(line); // writes csv data + new line
            await bufferWriter.FlushAsync();

            buffer.Position = 0; // go back to the start, where the data was written to
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

    // The culture here is relevant but only for the decimal point and invariant uses '.' so it's fine.
    private static CsvConfiguration CreateCsvOptions() => new(CultureInfo.InvariantCulture)
        {
            // one of those cases that, yes, could be made configurable but the output of the heating unit
            // is well known and won't change, so it's just more unnecessary code.
            Delimiter = ";",
            NewLine = NewLine,
            IgnoreBlankLines = true,
            HasHeaderRecord = false,
            CountBytes = false, // also makes sure encoding isn't required
            MemberTypes = MemberTypes.Properties,
            AllowComments = false,
        };
}
