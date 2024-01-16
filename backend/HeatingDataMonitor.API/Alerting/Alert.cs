using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;

namespace HeatingDataMonitor.API.Alerting;

public abstract class Alert : IAlert
{
    protected bool SuppressNotifications;
    public Notification? PendingNotification { get; protected set; }

    public abstract void Update(HeatingData data);

    public virtual void MarkAsSent()
    {
        SuppressNotifications = true;
        PendingNotification = null;
    }
}
