using System.Diagnostics;
using HeatingDataMonitor.Alerting.Alerts;
using HeatingDataMonitor.Database.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using NodaTime;
using Xunit.Abstractions;

namespace HeatingDataMonitor.Alerting.Tests.Alerts;

[TestSubject(typeof(AddFuelAlert))]
public class AddFuelAlertTests
{
    private readonly ITestOutputHelper _output;

    public AddFuelAlertTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ScriptedProcedure()
    {
        IAlert alert = new AddFuelAlert(Options.Create(new AddFuelOptions
        {
            LowerBound = 80,
            UpperBound = 180,
            AddFuelTemperature = 125,
            RepeatIntervalMinutes = 5,
            RequiredTemperatureDelta = 10,
            NumShouldAddFuel = 2
        }));

        (float temp, bool fire)[] dataPoints =
        {
            // data points spaced by one minute
            (100, false),
            (150, false),
            (180, false), // primed
            (200, false),
            (150, false),
            (125, true), // fired first time
            (120, false), // 1 min ago
            (115, false), // 2 min ago
            (110, false), // 3 min ago
            (105, false), // 4 min ago
            (100, true), // 5 min ago, repeat -> fired second time
            (90, false), // 1 min ago
            (95, false), // 2 min ago
            (100, false), // 3 min ago, unprimed because 10° temperature increase
            (105, false), // 4 min ago
            (110, false), // 5 min ago, but not primed
            (115, false), // 6 min ago, but not primed
            (150, false), // climb back up
            (180, false), // primed again
            (150, false), // falling back down
            (125, true), // fired third time
            (150, false), // climb back up
            (180, false), // primed again
            (150, false), // falling back down
            (125, false), // reached critical point again but already notified twice so no more
            (120, false), // falling down further
            (90, false), // falling down further
            (80, false), // reset entirely
            (50, false), // keep going down for a bit just to be sure
            (10, false), // keep going down for a bit just to be sure
        };

        Instant time = SystemClock.Instance.GetCurrentInstant();
        // if the alert resets properly this is cyclical, so we repeat this entire sequence twice in the test
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < dataPoints.Length; j++)
            {
                (float temp, bool fire) = dataPoints[j];
                _output.WriteLine($"At data point {j} in iteration {i}: ({temp:F1}, {fire})");
                alert.Update(new HeatingData
                {
                    ReceivedTime = time,
                    Abgas = temp,
                    Betriebsphase_Kessel = BetriebsPhaseKessel.Automatik,
                });

                if (fire)
                {
                    Assert.NotNull(alert.PendingNotification);
                    alert.MarkAsSent();
                }

                Assert.Null(alert.PendingNotification);

                time += Duration.FromMinutes(1);
            }
        }
    }
}
