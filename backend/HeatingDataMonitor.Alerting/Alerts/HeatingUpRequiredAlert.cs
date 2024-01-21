using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Notifications;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.Alerting.Alerts;

/// <summary>
/// Alert to let users know when they should fire up the heating unit
/// (first 'suggested', then the more urgent 'required').
/// Notifications are sent periodically until the heating unit is warm again.
/// </summary>
public class HeatingUpRequiredAlert : Alert
{
    private readonly bool _summerMode;
    private readonly int _suggestedThreshold;
    private readonly int _requiredThreshold;
    private readonly Duration _timeBelowThreshold;
    private readonly Duration _reminderDuration;

    private Instant? _lastAboveSuggested;
    private Instant? _lastAboveRequired;
    private Instant? _lastNotificationTriggered;
    private bool _notFallenBelowRequired;

    public HeatingUpRequiredAlert(IOptions<HeatingUpRequiredOptions> options)
    {
        _summerMode = options.Value.SummerMode;
        _suggestedThreshold = options.Value.SuggestedThreshold;
        _requiredThreshold = options.Value.RequiredThreshold;
        _timeBelowThreshold = Duration.FromMinutes(options.Value.MinutesBelowThreshold);
        _reminderDuration = Duration.FromHours(options.Value.ReminderHours);
    }

    public override void Update(HeatingData data)
    {
        // In Summer mode, only the Boiler is relevant
        // In Winter mode, you need to heat up when either Boiler or Puffer is below
        string offendingTemperature = "Boiler";
        float value = data.Boiler_1;
        if (!_summerMode && data.Puffer_Oben < data.Boiler_1)
        {
            value = data.Puffer_Oben;
            offendingTemperature = "Puffer";
        }

        if (value >= _suggestedThreshold)
        {
            _lastAboveSuggested = data.ReceivedTime;
            // start sending notifications again once temperature is high enough for heating to not be suggested anymore
            // also clear pending notification if there was one that was not sent (very unlikely in correct operation).
            SuppressNotifications = false;
            PendingNotification = null;
            _notFallenBelowRequired = true;
        }

        if (value >= _requiredThreshold)
        {
            _lastAboveRequired = data.ReceivedTime;
        }
        else if (_notFallenBelowRequired || data.ReceivedTime - _lastNotificationTriggered >= _reminderDuration)
        {
            // send notifications again when temperature falls below the required threshold for the first time or
            // if a lot of time has passed since the last notification was sent.
            // This will also re-trigger the notification periodically when the temperature stays low for a long time.
            SuppressNotifications = false;
            _notFallenBelowRequired = false;
        }

        CheckNotification(data.ReceivedTime, data, value, offendingTemperature);
    }

    private void CheckNotification(Instant now, HeatingData data, float value, string offendingTemperature)
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
            PendingNotification = BuildNotification(required: true, deltaRequired.Value, value, _requiredThreshold,
                offendingTemperature);
            _lastNotificationTriggered = data.ReceivedTime;
        }
        else if (deltaSuggested >= _timeBelowThreshold)
        {
            PendingNotification = BuildNotification(required: false, deltaSuggested.Value, value, _suggestedThreshold,
                offendingTemperature);
            _lastNotificationTriggered = data.ReceivedTime;
        }
    }

    // This could be templated to allow customization and localization, but that would be overkill for this project
    private static Notification BuildNotification(bool required, Duration delta, float temp, int threshold,
        string offendingTemperature) =>
        new("Aafüüre " + (required ? "nötig!" : "empfohle"),
            (string)(
                $"{offendingTemperature} isch sit {((int)delta.TotalHours > 0 ? $"{(int)delta.TotalHours} stung u " : "")}{delta.Minutes} minute unger {threshold}° C. " +
                $"Iz gad isch si {temp:F1}°."));
}