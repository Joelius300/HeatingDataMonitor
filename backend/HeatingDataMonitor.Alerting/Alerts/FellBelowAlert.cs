using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Notifications;
using NodaTime;

namespace HeatingDataMonitor.Alerting.Alerts;

/// <summary>
/// Alert to publish notification when a value has fallen below a threshold for some time. Notification can optionally
/// be repeated while it stays below the threshold.
/// </summary>
// Tracks the time since it was last above the threshold. When updated often enough there is virtually no difference
// to tracking time since first observation below threshold, but it might be a relevant detail.
public class FellBelowAlert : Alert
{
    public delegate Notification NotificationBuilder(HeatingData data, float value, float threshold, Duration delta);

    private readonly Func<HeatingData, float> _valueGetter;
    private readonly float _threshold;
    private readonly Duration _timeThreshold;
    private readonly Duration? _repeatAfter;
    private readonly NotificationBuilder _notificationBuilder;

    private Instant _lastAboveThreshold;
    private Instant _lastNotificationSent;

    public FellBelowAlert(Func<HeatingData, float> valueGetter, float threshold, Duration timeThreshold,
                          Duration? repeatAfter, NotificationBuilder notificationBuilder)
    {
        _valueGetter = valueGetter;
        _threshold = threshold;
        _timeThreshold = timeThreshold;
        _repeatAfter = repeatAfter;
        _notificationBuilder = notificationBuilder;
    }

    public override void Update(HeatingData data)
    {
        Instant now = data.ReceivedTime;
        float value = _valueGetter(data);
        if (value >= _threshold)
        {
            // above threshold, we are ready to send notification
            SuppressNotifications = false;
            _lastAboveThreshold = now;
        }

        if (_repeatAfter.HasValue && now - _lastNotificationSent >= _repeatAfter)
        {
            // if we should repeat the notification after a while and that while has passed, stop suppressing
            SuppressNotifications = false;
        }

        // No need to send notifications when the heating unit is running (but heat hasn't been transferred yet)
        if (SuppressNotifications || data.Betriebsphase_Kessel != BetriebsPhaseKessel.Aus)
            return;

        Duration delta = now - _lastAboveThreshold;
        if (delta < _timeThreshold)
            return;

        // if it was below threshold for long enough, publish notification
        PendingNotification = _notificationBuilder(data, value, _threshold, delta);
        _lastNotificationSent = now;
    }
}
