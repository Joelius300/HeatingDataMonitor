using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataHandler.Services
{
    public sealed class MockDataService : DataService
    {
        private readonly int _timeout;
        private readonly string _filePath;
        private readonly ILogger<MockDataService> _logger;
        private readonly string[] _fakeSPSOutput;
        private int _currentIndex = 0;

        public MockDataService(DataStorage dataStorage, IOptions<HeatingMonitorOptions> config, ILogger<MockDataService> logger) : base(dataStorage, logger)
        {
            _logger = logger;
            _timeout = config.Value.ExpectedReadIntervalInSeconds;
            _filePath = config.Value.SerialPortName;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"The file with sample data couldn't be found at '{_filePath}'.", _filePath);

            _fakeSPSOutput = File.ReadAllLines(_filePath);
            if (_fakeSPSOutput.Length < 5)
                throw new ArgumentException("The specified file needs to have at least 5 rows of sample data.");
        }

        protected override async Task<Data> GetNewData(CancellationToken cancellationToken)
        {
            if (_currentIndex >= _fakeSPSOutput.Length)
                _currentIndex = 0;

            string fakeDataCSV = string.Empty;
            try
            {
                _logger.LogInformation($"Delaying for {_timeout} seconds for effect.");
                await Task.Delay(TimeSpan.FromSeconds(_timeout), cancellationToken);

                fakeDataCSV = _fakeSPSOutput[_currentIndex++]; // take and increment
                _logger.LogInformation("Returning fake data from csv line: ");
                _logger.LogInformation(fakeDataCSV);
                return Data.FromSerialData(fakeDataCSV);
            }
            catch (TaskCanceledException e)
            {
                throw new NoDataReceivedException(e);
            }
            catch (FormatException e)
            {
                _logger.LogWarning($"Data wasn't formatted correctly: {e.Message}");

                throw new FaultyDataReceivedException(fakeDataCSV);
            }
        }
    }
}
