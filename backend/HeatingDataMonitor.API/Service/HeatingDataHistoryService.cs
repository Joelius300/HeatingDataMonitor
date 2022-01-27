using HeatingDataMonitor.API.Hubs;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Models;
using HeatingDataMonitor.Receiver;
using Microsoft.AspNetCore.SignalR;

namespace HeatingDataMonitor.API.Service;

public sealed class HeatingDataHistoryService : BackgroundService
{
    private const int ArchiveDelay = 60 * 1000;
    private readonly ILogger<HeatingDataHistoryService> _logger;
    private readonly IHubContext<HeatingDataHub, IHeatingDataHubClient> _hubContext;
    private readonly IHeatingDataReceiver _heatingDataReceiver;
    private readonly IServiceScopeFactory _scopeFactory;
    private HeatingData? _lastAdded;

    public HeatingDataHistoryService(ILogger<HeatingDataHistoryService> logger,
        IHubContext<HeatingDataHub, IHeatingDataHubClient> hubContext,
        IHeatingDataReceiver heatingDataReceiver,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _hubContext = hubContext;
        _heatingDataReceiver = heatingDataReceiver;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TaskCompletionSource<bool> tcs = new();
        _heatingDataReceiver.DataReceived += ReceivedHandler;

        // Wait for the first time DataReceived is fired, then start
        // the fixed time loop.
        await tcs.Task;
        await ExecuteLoop(stoppingToken);

        void ReceivedHandler(object? o, HeatingData e)
        {
            tcs.SetResult(true);
            _heatingDataReceiver.DataReceived -= ReceivedHandler;
        }
    }

    private async Task ExecuteLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            HeatingData? current = _heatingDataReceiver.Current;
            if (current != null && !ReferenceEquals(_lastAdded, current))
            {
                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    HeatingDataDbContext context = scope.ServiceProvider.GetRequiredService<HeatingDataDbContext>();
                    context.HeatingData.Add(current);
                    // Since the archiving is only done every few seconds at most, calling SaveChanges for every row shouldn't be an issue
                    await context.SaveChangesAsync(stoppingToken);
                }

                // In the very unlikely event that the archiving cycle is faster than the heating unit, don't trip
                _lastAdded = current;

                await _hubContext.Clients.All.OnDataPointArchived(current);
                _logger.LogDebug("Archived data with timestamp: {Timestamp}", current.ReceivedTime);
            }

            try
            {
                await Task.Delay(ArchiveDelay, stoppingToken);
            }
            catch (TaskCanceledException e)
            {
                _logger.LogDebug(e.Message);
                break;
            }
        }
    }
}
