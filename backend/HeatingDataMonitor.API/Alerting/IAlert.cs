using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;

namespace HeatingDataMonitor.API.Alerting;

public interface IAlert
{
    /// Update alert with current data and check triggers.
    void Update(HeatingData data);

    /// Mark alert notifications as sent.
    void MarkAsSent();

    /// The notification that should be sent from this alert (if any).
    Notification? PendingNotification { get; }
}
