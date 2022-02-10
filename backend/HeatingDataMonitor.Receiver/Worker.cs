using HeatingDataMonitor.Models;

namespace HeatingDataMonitor.Receiver;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHeatingDataReceiver _receiver;

    public Worker(ILogger<Worker> logger, IHeatingDataReceiver receiver)
    {
        _logger = logger;
        _receiver = receiver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (HeatingData record in _receiver.StreamHeatingData(stoppingToken))
        {
            _logger.LogInformation("Record received: {Record}", System.Text.Json.JsonSerializer.Serialize(record));
        }
    }
}
