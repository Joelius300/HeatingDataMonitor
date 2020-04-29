using CsvHelper;
using CsvHelper.Configuration;
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
    internal class SerialPortHeatingDataReceiver : IHeatingDataReceiver, IDisposable
    {
        private readonly SerialPortOptions _serialPortOptions;
        private readonly CsvConfiguration _csvConfig;
        private readonly ILogger<SerialPortHeatingDataReceiver> _logger;
        private readonly SerialPort _serialPort;
        private readonly Thread _readingThread;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _cancelToken;
        private readonly SerialDataReceivedEventHandler _dataReceivedHandler;
        private CsvReader _csvReader;
        private bool _disposed = false;

        public HeatingData Current => throw new NotImplementedException();
        public event EventHandler<HeatingData> DataReceived;

        public SerialPortHeatingDataReceiver(IOptions<SerialPortOptions> serialPortOptions, ILogger<SerialPortHeatingDataReceiver> logger)
        {
            _serialPortOptions = serialPortOptions.Value;
            _logger = logger;

            //  TODO make the options (SerialPortOptions) less specific and include the csv options too.
            // things like encoding and newline can be reused. also use newline enum in options instead of string.
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // from options
                NewLine = NewLine.CR,
                BadDataFound = c => _logger.LogWarning($"Bad data found at pos {c.CharPosition}: {c.RawRecord}"),
                IgnoreBlankLines = true,
                IgnoreQuotes = true,
                Encoding = Encoding.ASCII,
                HasHeaderRecord = false
            };
            _csvConfig.RegisterClassMap<HeatingDataCsvMap>();

            _cts = new CancellationTokenSource();
            _cancelToken = _cts.Token;
            _serialPort = CreateSerialPort();
            _dataReceivedHandler = SerialDataReceived;
            _serialPort.DataReceived += _dataReceivedHandler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _serialPort.Open();
            _csvReader = new CsvReader(new StreamReader(_serialPort.BaseStream), _csvConfig);
            _readingThread.Start(_cts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();

            return Task.CompletedTask;
        }

        private void Read(object cancelTokenObj)
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                _csvReader.Read(); // how to cancel?

                //int readAttempts = _serialPortOptions.ReadTimeoutMs / _serialPort.ReadTimeout;
                //string line = null;
                //for (int i = 0; i < readAttempts; i++)
                //{
                //    try
                //    {
                //        line = _serialPort.ReadLine();
                //    }
                //    catch (TimeoutException)
                //    {
                //        if (_cancelToken.IsCancellationRequested)
                //        {
                //            return; // exit early
                //        }
                //    }
                //}

                //if (line == null)
                //{
                //    _logger.LogInformation($"No serial data received in the last {_serialPortOptions.ReadTimeoutMs}ms.");
                //}
                //else
                //{
                //    OnDataReceived(line);
                //}
            }
        }

        protected virtual void OnDataReceived(string line)
        {
            try
            {
                
            }
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            _serialPort.ReadExisting();
        }

        private SerialPort CreateSerialPort()
        {
            string portName = _serialPortOptions.PortName;
            if (string.IsNullOrWhiteSpace(portName))
                throw new InvalidOperationException("The specified serial port name is invalid.");

            if (!SerialPort.GetPortNames().Contains(portName))
                throw new InvalidOperationException($"The specified serial port name '{portName}' was not found.");

            return new SerialPort()
            {
                PortName = portName,
                BaudRate = _serialPortOptions.BaudRate,
                DataBits = _serialPortOptions.DataBits,
                Parity = _serialPortOptions.Parity,
                Handshake = _serialPortOptions.Handshake,
                StopBits = _serialPortOptions.StopBits,
                Encoding = Encoding.GetEncoding(_serialPortOptions.Encoding),
                DiscardNull = true,
                ReadTimeout = 500, // allows graceful shutdown
                NewLine = _serialPortOptions.NewLine
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _serialPort.DataReceived -= _dataReceivedHandler;
                    _serialPort.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
