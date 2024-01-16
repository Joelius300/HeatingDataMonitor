using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;

namespace HeatingDataMonitor.API.Alerting;

public interface IAlert
{
    void Update(HeatingData data);
    void MarkAsSent();
    Notification? PendingNotification { get; }
}
