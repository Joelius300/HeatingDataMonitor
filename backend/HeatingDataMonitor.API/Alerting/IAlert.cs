using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;

namespace HeatingDataMonitor.API.Alerting;

/// <summary>
/// An alert monitors one or more variables whenever update is called and populates the notification property.
/// It relies on outside services to publish those notifications and telling it afterwards for resetting.
/// </summary>
public interface IAlert
{
    /// Update alert with current data and check triggers.
    void Update(HeatingData data);

    /// Mark alert notifications as sent.
    void MarkAsSent();

    /// The notification that should be sent from this alert (if any).
    Notification? PendingNotification { get; }
}
