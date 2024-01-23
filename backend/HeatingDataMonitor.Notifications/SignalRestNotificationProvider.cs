using System.Text;
using Microsoft.Extensions.Options;

namespace HeatingDataMonitor.Notifications;

internal class SignalRestNotificationProvider : INotificationProvider
{
    private readonly SignalNotificationOptions _options;
    private readonly ISignalRestClient _restClient;

    public SignalRestNotificationProvider(IOptions<SignalNotificationOptions> options, ISignalRestClient restClient)
    {
        _options = options.Value;
        _restClient = restClient;
    }

    public Task Publish(Notification notification) => _restClient.SendMessage(GetMessage(notification));

    private SignalMessageModel GetMessage(Notification notification) => new()
    {
        Message = $"**{notification.Title}**\n\n{notification.Text}",
        TextMode = "styled",
        Number = _options.Sender,
        Recipients = new[]
        {
            GroupIdToRestApiGroupId(_options.GroupId)
        }
    };

    // For some reason the devs of signal-cli-rest-api decided the base64 encoded group id was
    // not enough for the identification of the group so they decided to base64 encode it again,
    // but not the underlying bytes, oh no no, the actual character bytes from the UTF-8 glyphs.
    // The prefix I can understand but the rest is just completely useless and only increases
    // payload size and number of operations with no added benefit. Still, the show must go on.
    private static string GroupIdToRestApiGroupId(string groupId) =>
        $"group.{Convert.ToBase64String(Encoding.UTF8.GetBytes(groupId))}";
}
