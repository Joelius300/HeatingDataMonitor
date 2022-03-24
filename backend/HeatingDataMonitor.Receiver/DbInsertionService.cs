using System.Diagnostics;
using System.Threading.Channels;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Database.Write;
using HeatingDataMonitor.Receiver.Shared;
using Microsoft.Extensions.Options;

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
    private readonly DbResilienceOptions _resilienceOptions;

    public DbInsertionService(IHeatingDataWriteRepository repository, IHeatingDataReceiver receiver, ILogger<DbInsertionService> logger, IOptions<DbResilienceOptions> resilienceOptions)
    {
        _repository = repository;
        _receiver = receiver;
        _logger = logger;
        _resilienceOptions = resilienceOptions.Value;
    }

    // this queue and retry routine allows the db to be updated or restarted without losing data (if fast enough).
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan maxRetryTime = TimeSpan.FromMinutes(_resilienceOptions.RetryDurationMinutes);

        // continuously enqueue until service cancellation, db retry timeout or data blockage
        Channel<HeatingData> queue = CreateQueue(approximateCapacity: maxRetryTime.Add(TimeSpan.FromMinutes(2)));
        using CancellationTokenSource enqueuingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        Task enqueuingTask = WriteToQueue(queue, enqueuingCts.Token);

        Exception? lastExceptionAfterMaxRetries = null;

        // exceptions during data retrieval will be thrown by ReadAllAsync (internally by completing the queue with an ex).
        // for such exceptions there's no need to wait for the task to finish (it already has) so just let it bubble up.
        await foreach (HeatingData record in queue.Reader.ReadAllAsync(stoppingToken))
        {
            lastExceptionAfterMaxRetries = await TryInsertRecordWithRetry(record,
                TimeSpan.FromSeconds(_resilienceOptions.RetryIntervalSeconds),
                maxRetryTime, stoppingToken); // throws on cancellation, Host ignores that for BackgroundServices

            if (lastExceptionAfterMaxRetries is not null)
            {
                // couldn't insert data, even after maximum retries -> stop enqueuing more
                enqueuingCts.Cancel();
                break;
            }
        }

        // give 200ms for the enqueuing task to finish as well before finishing (either gracefully or with last exception)
        Task firstTask = await Task.WhenAny(enqueuingTask, Task.Delay(200, CancellationToken.None));
        Debug.Assert(firstTask == enqueuingTask, "firstTask == enqueuingTask");

        if (lastExceptionAfterMaxRetries is not null)
        {
            // at this point, data will be lost. In the case of this application it's not that big of a deal.
            _logger.LogCritical("Record couldn't be inserted after maximum number of retries; aborting with last exception");
            throw lastExceptionAfterMaxRetries;
        }
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
            // make ReadAllAsync throw this exception
            queue.Writer.TryComplete(error);
        }
    }

    /// <summary>
    /// Tries to insert a given record for a certain time with a certain retry interval. If the record couldn't be inserted
    /// during the specified <paramref name="maxRetryTime"/>, the method will return the last exception that prevented insertion.
    /// If insertion was successful, this method will return null.
    /// <para>
    /// THIS METHOD THROWS WHEN CANCELLATION IS REQUESTED. Bold because I usually hate that but here it does make sense.
    /// </para>
    /// </summary>
    private async Task<Exception?> TryInsertRecordWithRetry(HeatingData record, TimeSpan retryInterval, TimeSpan maxRetryTime, CancellationToken cancellationToken)
    {
        Exception? latestException = null;
        int retryCount = 0;
        int maxRetryCount = (int)(maxRetryTime.TotalSeconds / retryInterval.TotalSeconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _repository.InsertRecordAsync(record);
                break;
            }
            catch (Exception ex)
            {
                if (latestException is null || latestException.Message != ex.Message)
                {
                    _logger.LogError(ex, "DB insertion failed for record with timestamp {Timestamp}; retry in {NextRetrySeconds}s " +
                                         "(identical exceptions from consecutive retries are only logged once)",
                        record.ReceivedTime, retryInterval.TotalSeconds);
                }
                else
                {
                    _logger.LogDebug("DB insertion failed again after retry of {IntervalSeconds} seconds",
                        retryInterval.TotalSeconds);
                }

                latestException = ex;

                if (++retryCount >= maxRetryCount)
                    return latestException; // reached maxed retries; abort

                await Task.Delay(retryInterval, cancellationToken); // throws on cancellation
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogTrace("Added record to database with timestamp {Timestamp}", record.ReceivedTime);

        if (retryCount > 0)
        {
            _logger.LogInformation("DB insertion succeeded after {RetryCount} retries of {IntervalSeconds} seconds",
                retryCount, retryInterval.TotalSeconds);
        }

        return null; // if it was inserted eventually, we don't care for the exceptions
    }

    /// <summary>
    /// Creates a queue which can hold approximately <paramref name="approximateCapacity"/> worth of data.
    /// The queue clogs/blocks meaning new items simply can't be written to it until previous ones are consumed.
    /// </summary>
    /// <param name="approximateCapacity">
    /// A timespan defining how much data the queue should approximately be able to
    /// hold when expecting a new item every {configurable} seconds.
    /// </param>
    private Channel<HeatingData> CreateQueue(TimeSpan approximateCapacity) =>
        Channel.CreateBounded<HeatingData>(
            new BoundedChannelOptions((int)(approximateCapacity.TotalMilliseconds /
                                            _resilienceOptions.ExpectedNewRecordIntervalMilliseconds))
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
}
