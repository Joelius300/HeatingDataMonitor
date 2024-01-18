using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Notifications;

namespace HeatingDataMonitor.Alerting;

public abstract class Alert : IAlert
{
    /// <summary>
    /// Whether to suppress notifications or not. Set to true when notification was sent.
    /// Apart from that it has to be managed in the subclass.
    /// </summary>
    protected bool SuppressNotifications;
    public Notification? PendingNotification { get; protected set; }

    public abstract void Update(HeatingData data);

    public virtual void MarkAsSent()
    {
        SuppressNotifications = true;
        PendingNotification = null;
    }
}
