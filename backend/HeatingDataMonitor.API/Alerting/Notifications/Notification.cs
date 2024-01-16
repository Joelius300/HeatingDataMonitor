namespace HeatingDataMonitor.API.Alerting.Notifications;

/// <summary>
/// Notification that can be pushed to users via some notification provider.
/// </summary>
public class Notification
{
    public string Title { get; }
    public string Text { get; }

    public Notification(string title, string text)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public override string ToString() => $"{Title} | {Text}";
}
