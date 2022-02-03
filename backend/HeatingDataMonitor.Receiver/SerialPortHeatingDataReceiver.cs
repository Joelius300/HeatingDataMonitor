﻿using System.Globalization;
using System.IO.Ports;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.Receiver;

// TODO This and MockHeatingDataReceiver need to be refactored.
// They should be split so that one class is the receiver which
// hands over CSV-data line-by-line to a parser. One impl for SerialPort, one for File.
// The other is said parser, which parses the records and notifies anyone interested.
// We can use IAsyncEnumerable for this and I think the parser should call the receiver
// not the other way around because the parser will stay the same.
// Alternatively, a base class for the receiver with CSV and notification logic would do the same.
internal sealed class SerialPortHeatingDataReceiver : IHeatingDataReceiver, IDisposable
{

    public HeatingData? Current { get; private set; }
    public event EventHandler<HeatingData>? DataReceived;

    public SerialPortHeatingDataReceiver(IOptions<SerialPortOptions> options, ILogger<SerialPortHeatingDataReceiver> logger, IClock clock)
    {
        _options = options.Value;
        _logger = logger;

        // TODO Improve csvReader instantiation logic (refactor method)
        _csvConfig = CreateCsvOptions();
        _serialPort = CreateSerialPort();
        _newLineSplitArray = new[] { _serialPort.NewLine };
        _buffer = new MemoryStream();
        _bufferReader = new StreamReader(_buffer, _serialPort.Encoding);
        _bufferWriter = new StreamWriter(_buffer, _serialPort.Encoding);
        _csvReader = new CsvReader(_bufferReader, _csvConfig);
        _csvReader.Context.RegisterClassMap<HeatingDataCsvMap>();
        _clock = clock;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Stop();

        return Task.CompletedTask;
    }

    private void Start()
    {
        _serialPort.Open();
        _serialPort.DataReceived += DataReceivedHandler;
    }

    private void Stop()
    {
        _serialPort.DataReceived -= DataReceivedHandler;
        _serialPort.Close();
    }

    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        // Let's forget the fancy stuff for now. Who cares if we encode and decode this data
        // 5 more times than necessary if the data only appears every few seconds.
        // Performance is definitely not a driving enough factor to go crazy with this.
        // It would be very nice to have a custom CsvParser that continues where it left
        // off and acts conservative meaning it only finishes a field when it encounters
        // a field separator and it only finishes a line when it encounters a new line.
        // EOF would simply mean 'the rest will come soon'. It's definitely possible to implement
        // and would give a boost to performance along with simply being a cool thing
        // to implement.
        // YAGNI and KISS are more important than I like to admit so let's keep it simple for now.

        string currentData = _serialPort.ReadExisting();
        string fullData = _unfinishedLine + currentData;
        string[] lines = fullData.Split(_newLineSplitArray, StringSplitOptions.None);
        if (lines.Length == 1)
        {
            // There's no line-break, just add to the unfinished line
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

            // with this procedure we only want to process the data if there's a whole line
            ProcessBufferedData();
        }
    }

    private void ProcessBufferedData()
    {
        _buffer.Position = 0;

        while (_csvReader.Read())
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
                if (e.InnerException is not null)
                {
                    sb.AppendLine($"Reason: {e.InnerException.Message}");
                }
                sb.AppendLine($"Record: {e.Context.Parser.RawRecord}");
                _logger.LogError(sb.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception while reading data - Aborting");
                Stop();
                break;
            }

            if (record is not null)
            {
                record.ReceivedTime = _clock.GetCurrentInstant();
                OnDataReceived(record);
            }
        }
    }

    private void OnDataReceived(HeatingData data)
    {
        Current = data;
        DataReceived?.Invoke(this, data);
    }

}
