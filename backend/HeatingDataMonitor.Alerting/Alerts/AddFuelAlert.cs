using HeatingDataMonitor.Database.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.Alerting.Alerts;

/// <summary>
/// Alert to notify users that they should add fuel to the running heating unit for a second heating process.
/// It primes itself on some upper threshold, waits until it falls below a critical point and then raises.
/// Once the heating unit is turned off or the temperature falls below some lower threshold, it is reset.
/// Can be configured to suggest adding fuel x times because after 1 or 2 the Boiler and Puffer is full usually.
/// </summary>
public class AddFuelAlert : Alert
{
    private readonly AddFuelOptions _options;
    private readonly Duration _repeatInterval;

    private Instant? _lastNotificationSent;
    private float? _lowestTemperatureSinceNotification;
    private bool _primed;
    private int _numAddedFuel;

    public AddFuelAlert(IOptions<AddFuelOptions> options)
    {
        _options = options.Value;
        _repeatInterval = Duration.FromMinutes(_options.RepeatIntervalMinutes);
    }

    public override void Update(HeatingData data)
    {
        if (data.Betriebsphase_Kessel == BetriebsPhaseKessel.Aus)
        {
            // this alert only is relevant when the heating unit is running
            Reset();
            return;
        }

        Instant now = data.ReceivedTime;
        if (now - _lastNotificationSent >= _repeatInterval && _numAddedFuel < _options.NumShouldAddFuel)
        {
            // after notification was published suppression is enabled until enough time has passed.
            // suppression is kept going if the number of times fuel should be added is already reached.
            SuppressNotifications = false;
        }

        if (data.Abgas >= _options.UpperBound)
        {
            // alert becomes active when heating unit is running close to peak
            _primed = true;
        }
        else if (_primed)
        {
            if (_lowestTemperatureSinceNotification.HasValue)
            {
                _lowestTemperatureSinceNotification = Math.Min(_lowestTemperatureSinceNotification.Value, data.Abgas);
            }

            if (_lowestTemperatureSinceNotification.HasValue &&
                data.Abgas - _lowestTemperatureSinceNotification >= _options.RequiredTemperatureDelta)
            {
                // someone added fuel and the temperature is rising again, un-prime alert and increase counter
                _primed = false;
                _numAddedFuel += 1;
                _lowestTemperatureSinceNotification = null;
            } // else -> declining temperature
            else if (data.Abgas <= _options.LowerBound)
            {
                // too cold, adding fuel is not worth it / might not work
                Reset();
            }
            else if (!SuppressNotifications && data.Abgas <= _options.AddFuelTemperature)
            {
                // ready for new notifications and temperature is in range where adding fuel is a good idea
                PendingNotification = NotificationBuilders.AddingFuelNecessary(data.Abgas);
                _lastNotificationSent = now;
                _lowestTemperatureSinceNotification = data.Abgas;
            }
        }
    }

    private void Reset()
    {
        PendingNotification = null;
        SuppressNotifications = false;
        _primed = false;
        _lastNotificationSent = null;
        _lowestTemperatureSinceNotification = null;
        _numAddedFuel = 0;
    }
}
