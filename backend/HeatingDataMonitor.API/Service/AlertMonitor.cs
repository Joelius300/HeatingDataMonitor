using HeatingDataMonitor.API.Alerting;
using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Receiver.Shared;

namespace HeatingDataMonitor.API.Service;

public class AlertMonitor : BackgroundService
{
    private readonly ILogger<AlertMonitor> _logger;
    private readonly IHeatingDataReceiver _receiver;
    private readonly ICollection<IAlert> _alerts;
    private readonly ICollection<INotificationProvider> _notificationProviders;

    public AlertMonitor(ILogger<AlertMonitor> logger, IHeatingDataReceiver heatingDataReceiver,
        ICollection<IAlert> alerts, ICollection<INotificationProvider> notificationProviders)
    {
        _logger = logger;
        _receiver = heatingDataReceiver;
        _alerts = alerts;
        _notificationProviders = notificationProviders;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (HeatingData data in _receiver.StreamHeatingData(stoppingToken))
        {
            // Update all alerts together before checking for notifications
            foreach (IAlert alert in _alerts)
            {
                alert.Update(data);
            }

            foreach (IAlert alert in _alerts)
            {
                if (alert.PendingNotification is null)
                    continue;

                try
                {
                    foreach (INotificationProvider provider in _notificationProviders)
                    {
                        provider.Publish(alert.PendingNotification);
                    }

                    // Notification is only reset when firing was successful, otherwise it'll stay and be fired again
                    // next iteration (unless update reset itself because the notification is no longer necessary).
                    alert.MarkAsSent();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Could not fire notification: '{Notification}'", alert.PendingNotification);
                }
            }
        }
    }
}
