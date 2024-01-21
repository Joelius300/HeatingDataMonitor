using HeatingDataMonitor.Alerting.Alerts;
using HeatingDataMonitor.Database.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.Alerting.Tests;

public class HeatingUpRequiredAlertTests
{
    [Fact]
    public void ScriptedTestSummer()
    {
        Alert alert = new HeatingUpRequiredAlert(Options.Create(new HeatingUpRequiredOptions
        {
            MinutesBelowThreshold = 5,
            ReminderHours = 1,
            RequiredThreshold = 30,
            SuggestedThreshold = 40,
            SummerMode = true,
        }));
        Instant now = Instant.FromUtc(2000, 1, 1, 0, 0);

        // --- Does not trigger

        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now,
            Boiler_1 = 50,
        });
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(2),
            Boiler_1 = 44,
        });
        Assert.Null(alert.PendingNotification);

        // --- Trigger suggested

        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(6),
            Boiler_1 = 39,
        });
        // last >=40 is 4 min ago -> not triggered yet
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(10),
            Boiler_1 = 38,
        });

        // last >=40 is 8 min ago -> should trigger
        Assert.NotNull(alert.PendingNotification);
        // Simulate sending
        alert.MarkAsSent();
        // no more notification after sending

        // --- Trigger required

        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(12),
            Boiler_1 = 35,
        });
        // <= 40 for 10 minutes now but no notification
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(15),
            Boiler_1 = 29,
        });
        // last >=40 was 13 min ago, last >=30 was 3 minutes ago -> not triggered yet
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(18),
            Boiler_1 = 28,
        });

        // last >=30 was 6 minutes ago -> should trigger
        Assert.NotNull(alert.PendingNotification);
        // Simulate sending
        alert.MarkAsSent();
        // no more notification after sending
        Assert.Null(alert.PendingNotification);

        // --- Reminder after one hour

        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(55),
            Boiler_1 = 25,
        });
        // last >=30 was 40 minutes ago but already sent -> does not trigger
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(75),
            Boiler_1 = 20,
        });
        // last >=30 was 60 minutes ago and already sent -> should trigger
        Assert.NotNull(alert.PendingNotification);
        // Simulate sending
        alert.MarkAsSent();
        // no more notification after sending
        Assert.Null(alert.PendingNotification);

        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(105),
            Boiler_1 = 20,
        });
        // last >=30 was 90 minutes ago, last since notif was 30 -> does not trigger
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(135),
            Boiler_1 = 20,
        });
        // last >=30 was 120 minutes ago, last since notif was 60 -> should trigger
        Assert.NotNull(alert.PendingNotification);
        // Simulate sending
        alert.MarkAsSent();
        // no more notification after sending
        Assert.Null(alert.PendingNotification);

        // --- Reset after it's higher up again

        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(140),
            Boiler_1 = 35,
        });
        // last >=30 was 0 minutes ago, last >=40 was ages ago but last notif was only 5 min ago -> no trigger
        Assert.Null(alert.PendingNotification);
        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(145),
            Boiler_1 = 45,
        });
        // >= 40 again, everything is reset so going down to 40 will trigger again
        Assert.Null(alert.PendingNotification);

        // --- Re-trigger after reset

        alert.Update(new HeatingData
        {
            ReceivedTime = now + Duration.FromMinutes(155),
            Boiler_1 = 39,
        });
        // last >= 40 was 10 min ago so it should trigger :)
        Assert.NotNull(alert.PendingNotification);
    }
}
