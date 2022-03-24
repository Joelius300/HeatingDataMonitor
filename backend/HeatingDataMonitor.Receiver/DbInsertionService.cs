using System.Diagnostics;
using System.Threading.Channels;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Database.Write;
using HeatingDataMonitor.Receiver.Shared;

namespace HeatingDataMonitor.Receiver;

/// <summary>
/// Inserts records from <see cref="IHeatingDataReceiver.StreamHeatingData(CancellationToken)"/>
/// into the database until the application is shut down.
/// </summary>
public class DbInsertionService : BackgroundService
{
    private readonly IHeatingDataWriteRepository _repository;
    private readonly IHeatingDataReceiver _receiver;
    private readonly ILogger<DbInsertionService> _logger;

    public DbInsertionService(IHeatingDataWriteRepository repository, IHeatingDataReceiver receiver, ILogger<DbInsertionService> logger)
    {
        _repository = repository;
        _receiver = receiver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: put the queue timespan and the maxretrytime in a variable, maybe even in a config, and just add a few
        // minutes for the queue one.
        Channel<HeatingData> queue = CreateQueue(TimeSpan.FromMinutes(15));
        Task enqueuingTask = WriteToQueue(queue, stoppingToken); // continuously enqueue until cancellation or data blockage

        await foreach (HeatingData record in queue.Reader.ReadAllAsync(stoppingToken))
        {
            // try to insert the record but retry for 10 minutes if the db isn't working.
            // this allows the db to be updated or restarted without losing data.
            await InsertRecordWithRetry(record, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(10), stoppingToken);
        }

        // give 500ms for the enqueuing task to finish as well before "officially" being done
        Task firstTask = await Task.WhenAny(enqueuingTask, Task.Delay(500, CancellationToken.None));
        Debug.Assert(firstTask == enqueuingTask, "firstTask == enqueuingTask");
    }

    private async Task WriteToQueue(Channel<HeatingData> queue, CancellationToken stoppingToken)
    {
        Exception? error = null;
        try
        {
            await foreach (HeatingData record in _receiver.StreamHeatingData(stoppingToken))
            {
                if (!queue.Writer.TryWrite(record))
                    throw new InvalidOperationException(
                        "Queue is full even though db insertion should've failed enough times for the application to be stopped");
            }
        }
        catch (Exception ex)
        {
            error = ex;
        }
        finally
        {
            queue.Writer.TryComplete(error);
        }
    }

    private async Task InsertRecordWithRetry(HeatingData record, TimeSpan retryInterval, TimeSpan maxRetryTime, CancellationToken cancellationToken)
    {
        bool insertedSuccessfully = false;
        Exception? exception = null;
        int retryCount = 0;
        int maxRetryCount = (int)(maxRetryTime.TotalSeconds / retryInterval.TotalSeconds);
        do
        {
            try
            {
                await _repository.InsertRecordAsync(record);
            }
            catch (Exception ex)
            {
                if (exception is null || exception.Message != ex.Message)
                {
                    exception = ex;
                    _logger.LogError(ex, "DB insertion failed for record with timestamp {Timestamp}", record.ReceivedTime);
                }
                else
                {
                    _logger.LogDebug("DB insertion failed again after retry of {IntervalSeconds} seconds",
                        retryInterval.TotalSeconds);
                }

                retryCount++;
                await Task.Delay(retryInterval, cancellationToken);
                continue;
            }

            if (retryCount > 0)
            {
                _logger.LogInformation("DB insertion succeeded after {RetryCount} retries of {IntervalSeconds} seconds",
                    retryCount, retryInterval.TotalSeconds);
            }

            _logger.LogTrace("Added record to database with timestamp {Timestamp}", record.ReceivedTime);
            insertedSuccessfully = true;
        } while (!insertedSuccessfully && retryCount < maxRetryCount && !cancellationToken.IsCancellationRequested);

        if (retryCount >= maxRetryCount)
        {
            Debug.Assert(exception is not null, "exception is not null");
            throw exception;
        }
    }

    /// <summary>
    /// Creates a queue which can hold approximately <paramref name="approximateCapacity"/> worth of data.
    /// The queue clogs/blocks meaning new items simply can't be written to it until previous ones are consumed.
    /// </summary>
    /// <param name="approximateCapacity">A timespan defining how much data the queue should approximately be able to hold when expecting a new item every ~6 seconds.</param>
    private static Channel<HeatingData> CreateQueue(TimeSpan approximateCapacity) =>
        Channel.CreateBounded<HeatingData>(new BoundedChannelOptions((int)(approximateCapacity.TotalSeconds / 6))
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });
}
