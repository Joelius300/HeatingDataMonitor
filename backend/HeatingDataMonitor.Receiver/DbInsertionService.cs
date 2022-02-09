using HeatingDataMonitor.Database;
using HeatingDataMonitor.Models;

namespace HeatingDataMonitor.Receiver;

/// <summary>
/// Inserts records from <see cref="IHeatingDataReceiver.StreamHeatingData(CancellationToken)"/>
/// into the database until the application is shut down.
/// </summary>
public class DbInsertionService : BackgroundService
{
    private readonly IHeatingDataRepository _repository;
    private readonly IHeatingDataReceiver _receiver;
    private readonly ILogger<DbInsertionService> _logger;

    public DbInsertionService(IHeatingDataRepository repository, IHeatingDataReceiver receiver, ILogger<DbInsertionService> logger)
    {
        _repository = repository;
        _receiver = receiver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (HeatingData record in _receiver.StreamHeatingData(stoppingToken))
        {
            await _repository.InsertRecordAsync(record);
            _logger.LogTrace("Added record to database with timestamp {Timestamp}", record.ReceivedTime);
        }
    }
}
