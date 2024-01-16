using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.API.Alerting.Alerts;

/// <summary>
/// Alert to let users know when they should fire up the heating unit
/// (first 'suggested', then the more urgent 'required').
/// Notifications are sent periodically until the heating unit is warm again.
/// </summary>
public class HeatingUpRequiredAlert : Alert
{
    private readonly int _suggestedThreshold;
    private readonly int _requiredThreshold;
    private readonly Duration _timeBelowThreshold;
    private readonly Duration _reminderDuration;

    private readonly IClock _clock;

    private Instant? _lastAboveSuggested;
    private Instant? _lastAboveRequired;

    public HeatingUpRequiredAlert(IOptions<HeatingUpRequiredOptions> options, IClock clock)
    {
        _suggestedThreshold = options.Value.SuggestedThreshold;
        _requiredThreshold = options.Value.RequiredThreshold;
        _timeBelowThreshold = Duration.FromMinutes(options.Value.MinutesBelowThreshold);
        _reminderDuration = Duration.FromHours(options.Value.ReminderHours);
        _clock = clock;
    }

    public override void Update(HeatingData data)
    {
        // TODO
        // In Summer mode, only the Boiler is relevant
        // In Winter mode, you need to heat up when either Boiler or Puffer is below
        float value = data.Boiler_1;
        Instant now = _clock.GetCurrentInstant();
        if (value >= _suggestedThreshold)
        {
            _lastAboveSuggested = now;
            // start sending notifications again once temperature is warm enough for heating to not be suggested anymore
            SuppressNotifications = false;
        }

        if (value >= _requiredThreshold)
        {
            _lastAboveRequired = now;
        }
        else if (now - _lastAboveRequired >= _reminderDuration)
        {
            // also send notifications again if the temperature has been below the required threshold for a long time
            SuppressNotifications = false;
        }

        CheckNotification(now, data);
    }

    private void CheckNotification(Instant now, HeatingData data)
    {
        if (SuppressNotifications)
            return;

        // No need to send notifications when the heating unit is running (but heat hasn't been transferred yet)
        if (data.Betriebsphase_Kessel != BetriebsPhaseKessel.Aus)
            return;

        Duration? deltaRequired = now - _lastAboveRequired;
        Duration? deltaSuggested = now - _lastAboveSuggested;
        if (deltaRequired >= _timeBelowThreshold)
        {
            PendingNotification = BuildNotification(required: true, deltaRequired.Value, data.Boiler_1, _requiredThreshold);
        }
        else if (deltaSuggested >= _timeBelowThreshold)
        {
            PendingNotification = BuildNotification(required: false, deltaSuggested.Value, data.Boiler_1, _suggestedThreshold);
        }
    }

    // This could be templated to allow customization and localization but for this project that would be overkill
    private static Notification BuildNotification(bool required, Duration delta, float temp, int threshold) =>
        new("Aafüüre " + (required ? "nötig!" : "empfohle"),
            $"Temperatur isch sit {(delta.Hours > 0 ? $"{delta.Hours} stung u " : "")}{delta.Minutes} minute unger {threshold}° C. " +
            $"Iz gad isch si {temp:F1}°.");
}
