using CliWrap;
using NodaTime;

namespace HeatingDataMonitor.Notifications;

// Again, for a more sophisticated notification setup, I would use Apprise under the hood instead of doing this myself.
public class SignalCliNotificationProvider : INotificationProvider
{
    private readonly SignalNotificationOptions _options;
    private readonly IClock _clock;
    private readonly Duration _receiveInterval;

    private Instant? _lastReceived;

    public SignalCliNotificationProvider(SignalNotificationOptions options, IClock clock)
    {
        _options = options;
        _clock = clock;
        _receiveInterval = Duration.FromHours(options.ReceiveHours);
    }

    public async Task Publish(Notification notification)
    {
        await EnsureReceived();
        await SendNotification(notification);
    }

    private Task EnsureReceived()
    {
        Instant now = _clock.GetCurrentInstant();
        if (now - _lastReceived <= _receiveInterval) // also false if _lastReceive is null
            return Task.CompletedTask;

        _lastReceived = now;
        return Receive();
    }

    private Task SendNotification(Notification notification)
    {
        (string message, string style) = FormatNotification(notification);

        return StartSignalCliWith("send", "-m", message, "--text-style", style, "-g", _options.GroupId);
    }

    private Task Receive() => StartSignalCliWith("receive");

    private static (string message, string style) FormatNotification(Notification notification) => (
            $"{notification.Title}\n\n{notification.Text}", $"0:{notification.Title.Length}:BOLD");

    private CommandTask<CommandResult> StartSignalCliWith(params string[] args) => Cli.Wrap(_options.CliPath)
        .WithArguments(new[]
        {
            "-u",
            _options.Sender,
        }.Concat(args))
        .ExecuteAsync();
}
