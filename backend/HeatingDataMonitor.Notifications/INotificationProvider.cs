namespace HeatingDataMonitor.Notifications;

public interface INotificationProvider
{
    /// <summary>
    /// Publish/Send notification through the provider.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    Task Publish(Notification notification);
}
