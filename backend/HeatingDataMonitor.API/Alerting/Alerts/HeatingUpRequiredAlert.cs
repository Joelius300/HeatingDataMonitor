using HeatingDataMonitor.API.Alerting.Notifications;
using HeatingDataMonitor.Database.Models;
using NodaTime;

namespace HeatingDataMonitor.API.Alerting.Alerts;

/// <summary>
/// Alert to let users know when they should fire up the heating unit
/// (first 'suggested', then the more urgent 'required').
/// Notifications are sent periodically until the heating unit is warm again.
/// </summary>
public class HeatingUpRequiredAlert : IAlert
{
    // TODO move to options
    private readonly int _suggestedThreshold;
    private readonly int _requiredThreshold;
    // TODO make configurable
    private readonly Duration _timeBelowThreshold = Duration.FromMinutes(5);
    private readonly Duration _reminderDuration = Duration.FromHours(1);

    private readonly IClock _clock;
    // private readonly PropertyInfo _propertyAccessor;

    private bool _suppressNotifications;
    private Instant? _lastAboveSuggested;
    private Instant? _lastAboveRequired;

    public HeatingUpRequiredAlert(int suggestedThreshold, int requiredThreshold, IClock clock)
    {
        // _propertyAccessor = typeof(HeatingData).GetProperty(monitor, typeof(float)) ??
        //                     throw new ArgumentException($"Monitor property '{monitor}' not found in HeatingData.",
        //                         nameof(monitor));
        //
        _suggestedThreshold = suggestedThreshold;
        _requiredThreshold = requiredThreshold;
        _clock = clock;
    }

    public Notification? PendingNotification { get; private set; }

    public void Update(HeatingData data)
    {
        // TODO also account for buffer temperature (Puffer_Oben) maybe?
        float value = data.Boiler_1;
        Instant now = _clock.GetCurrentInstant();
        if (value >= _suggestedThreshold)
        {
            _lastAboveSuggested = now;
        }

        if (value >= _requiredThreshold)
        {
            _lastAboveRequired = now;
            // start sending notifications again once temperature is warm enough for heating to not be urgent anymore
            _suppressNotifications = false;
        }
        else if (now - _lastAboveRequired >= _reminderDuration)
        {
            // also send notifications again if the temperature has been below the required threshold for a long time
            _suppressNotifications = false;
        }

        CheckNotification(now, data);
    }

    private void CheckNotification(Instant now, HeatingData data)
    {
        if (_suppressNotifications)
            return;

        // No need to send notifications when the heating unit is running (but not hot enough yet)
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

    private static Notification BuildNotification(bool required, Duration delta, float temp, int threshold) =>
        new("Aafüüre " + (required ? "nötig!" : "empfohle"),
            $"Temperatur isch sit {(delta.Hours > 0 ? $"{delta.Hours} stung u " : "")}{delta.Minutes} minute unger {threshold}° C. " +
            $"Iz gad isch si {temp:F1}°.");

    public void MarkAsSent()
    {
        _suppressNotifications = true;
        PendingNotification = null;
    }
}
