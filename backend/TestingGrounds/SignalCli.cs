using HeatingDataMonitor.Notifications;
using NodaTime;

namespace TestingGrounds;

public static class SignalCli
{
    public static async Task SendSomeMessages()
    {
        INotificationProvider provider = new SignalCliNotificationProvider(new SignalNotificationOptions
            {
            },
            SystemClock.Instance);

        await provider.Publish(new Notification("und wöus so spass gmacht het nomau", "dr joel isch nämläch blööd"));
        await provider.Publish(new Notification("Joo das isch ä fettä Titu?", "Und das dr Body"));
        await Task.Delay(2 * 60 * 1000);
        await provider.Publish(new Notification("iz nomau", "reeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee aösdfj öaskfj öaslfkj aslfk jaöslfk jaslfk jsfdak jsödf lksj  700° C heiss heiss heiss autsch i teste gärn in produktion"));
    }
}
