namespace HeatingDataMonitor.Receiver;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ICsvHeatingDataReader _csvHeatingDataReader;

    public Worker(ILogger<Worker> logger, ICsvHeatingDataReader csvHeatingDataReader)
    {
        _logger = logger;
        _csvHeatingDataReader = csvHeatingDataReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (string line in _csvHeatingDataReader.ReadCsvLines().WithCancellation(stoppingToken))
        {
            _logger.LogInformation("Line received: {Line}", line);
        }
    }
}
