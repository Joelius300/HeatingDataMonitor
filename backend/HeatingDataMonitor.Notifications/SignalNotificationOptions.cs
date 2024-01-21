namespace HeatingDataMonitor.Notifications;

public class SignalNotificationOptions
{
    public string Sender { get; set; }
    public string GroupId { get; set; }
    public string CliPath { get; set; }

    /// How often to receive signal messages. Must be "regularly" to avoid fingerprint mismatches etc.
    public float ReceiveHours { get; set; }
}
