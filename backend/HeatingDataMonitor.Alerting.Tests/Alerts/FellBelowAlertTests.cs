using HeatingDataMonitor.Alerting.Alerts;
using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Notifications;
using JetBrains.Annotations;
using NodaTime;

namespace HeatingDataMonitor.Alerting.Tests.Alerts;

[TestSubject(typeof(FellBelowAlert))]
public class FellBelowAlertTests
{
    [Fact]
    public void ScriptedOperationTriggers()
    {
        Alert alert = new FellBelowAlert(hd => hd.Boiler_1, threshold: 30, Duration.FromMinutes(2), repeatAfter: null,
            (data, value, threshold, delta) => new Notification($"{value:F0}", $"{delta.TotalMinutes:F0}"));
        Instant now = Instant.FromUtc(2000, 1, 1, 0, 0);

        // starts without notification
        Assert.Null(alert.PendingNotification);

        alert.Update(new HeatingData {ReceivedTime = now, Boiler_1 = 31});
        // still >= threshold
        Assert.Null(alert.PendingNotification);

        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(1), Boiler_1 = 30});
        // still >= threshold
        Assert.Null(alert.PendingNotification);

        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(2), Boiler_1 = 29});
        // first time below threshold. 1 min since last seen above so no notification
        Assert.Null(alert.PendingNotification);

        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(3), Boiler_1 = 28});
        // second time below threshold. 2 min since last seen above so notification is fired
        Assert.NotNull(alert.PendingNotification);
        Assert.Equal("28", alert.PendingNotification!.Title); // temperature
        Assert.Equal("2", alert.PendingNotification!.Text); // delta
    }

    [Fact]
    public void MarkAsSendClearsNotification()
    {
        Alert alert = new FellBelowAlert(hd => hd.Boiler_1, threshold: 30, Duration.FromMinutes(2), repeatAfter: null,
            (data, value, threshold, delta) => new Notification($"{value:F0}", $"{delta.TotalMinutes:F0}"));
        Instant now = Instant.FromUtc(2000, 1, 1, 0, 0);
        alert.Update(new HeatingData {ReceivedTime = now, Boiler_1 = 35});
        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(10), Boiler_1 = 25});

        Assert.NotNull(alert.PendingNotification);
        Assert.Equal("25", alert.PendingNotification!.Title); // temperature
        Assert.Equal("10", alert.PendingNotification!.Text); // delta

        // Simulate sending
        alert.MarkAsSent();
        // No more pending notification after sending it
        Assert.Null(alert.PendingNotification);
    }

    [Fact]
    public void DoesNotRetriggerWithoutRepeat()
    {
        Alert alert = new FellBelowAlert(hd => hd.Boiler_1, threshold: 30, Duration.FromMinutes(2), repeatAfter: null,
            (data, value, threshold, delta) => new Notification($"{value:F0}", $"{delta.TotalMinutes:F0}"));
        Instant now = Instant.FromUtc(2000, 1, 1, 0, 0);
        alert.Update(new HeatingData {ReceivedTime = now, Boiler_1 = 35});
        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(10), Boiler_1 = 25});
        alert.MarkAsSent();
        // Was triggered and sent once already, should be reset

        // No matter how much time passes, the repetition doesn't trigger
        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromHours(10), Boiler_1 = 20});
        Assert.Null(alert.PendingNotification);
    }

    [Fact]
    public void DoesRetriggerWithRepeat()
    {
        Alert alert = new FellBelowAlert(hd => hd.Boiler_1, threshold: 30, Duration.FromMinutes(2),
            repeatAfter: Duration.FromMinutes(10),
            (data, value, threshold, delta) => new Notification($"{value:F0}", $"{delta.TotalMinutes:F0}"));
        Instant now = Instant.FromUtc(2000, 1, 1, 0, 0);

        alert.Update(new HeatingData {ReceivedTime = now, Boiler_1 = 30});
        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(5), Boiler_1 = 25});
        Assert.NotNull(alert.PendingNotification);
        alert.MarkAsSent();
        // Was triggered and sent once already, should be reset

        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(10), Boiler_1 = 25});
        // Still below after 10 minutes, only 5 since last notification though
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData {ReceivedTime = now + Duration.FromMinutes(15), Boiler_1 = 25});
        // Still below after 15 minutes, now 10 min since last notification so retrigger
        Assert.NotNull(alert.PendingNotification);
        Assert.Equal("25", alert.PendingNotification!.Title); // temperature
        Assert.Equal("15", alert.PendingNotification!.Text); // delta
    }
}
