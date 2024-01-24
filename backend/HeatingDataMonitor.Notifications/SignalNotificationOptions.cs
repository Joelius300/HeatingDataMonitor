namespace HeatingDataMonitor.Notifications;

public class SignalNotificationOptions
{
    // The full number of the sender including the country code. No spaces.
    public string Sender { get; set; }

    // The group id for the group to send to. Base64 encoded like you can get it with `signal-cli` or also through
    // the API. For testing, you can also use a phone number in the same format as the sender.
    public string GroupId { get; set; }
}
