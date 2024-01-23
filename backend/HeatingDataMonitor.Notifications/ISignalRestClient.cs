using Refit;

namespace HeatingDataMonitor.Notifications;

internal interface ISignalRestClient
{
    [Post("/v2/send")]
    Task SendMessage([Body] SignalMessageModel message);
}
