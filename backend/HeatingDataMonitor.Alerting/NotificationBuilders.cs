using HeatingDataMonitor.Database.Models;
using HeatingDataMonitor.Notifications;
using NodaTime;

namespace HeatingDataMonitor.Alerting;

/// <summary>
/// Helper methods to build notifications for specific alerts.
/// </summary>
// Deciding on the design for this was tricky but I think having one place to define these makes sense for DX.
// These could be templated to allow customization and localization, but that would be overkill for this project
public static class NotificationBuilders
{
    private static Notification BuildHeatingUpRequiredNotification(bool required, Duration delta, float temp,
                                                                   float threshold, string offendingTemperature) =>
        new("Aafüüre " + (required ? "dringend nötig!" : "wär guet!"),
            $"{offendingTemperature} isch sit " +
            ((int)delta.TotalHours > 0 ? $"{(int)delta.TotalHours} stung u " : "") +
            $"{delta.Minutes} minute unger {threshold:F1}° C. " +
            $"Iz gad isch si {temp:F1}°.");

    /// <summary>
    /// Builds a notification to tell you that you should heat up aka fire up the heating unit.
    /// If it's urgent (aka really required, not just suggested), then set required to true.
    /// </summary>
    /// <param name="required">Whether it's required (urgent) or just suggested to heat up.</param>
    /// <param name="delta">The time since the last observation above the threshold.</param>
    /// <param name="data">The current observation.</param>
    /// <param name="threshold">The threshold that was passed to cause this notification.</param>
    /// <param name="summerMode">Whether the heating unit is in summer mode. In summer mode only the Boiler is relevant
    /// while in winter mode also the Puffer temperature is seen as relevant. It's expected from the alert
    /// that is using this method to also use the lower of the two values, otherwise there could be mismatches!
    /// </param>
    public static Notification BuildHeatingUpRequiredNotification(bool required, Duration delta, HeatingData data,
                                                                  float threshold, bool summerMode)
    {
        // In Summer mode, only the Boiler is relevant
        // In Winter mode, you need to heat up when either Boiler or Puffer is below
        string offendingTemperature = "Boiler";
        float value = data.Boiler_1;
        if (!summerMode && data.Puffer_Oben < data.Boiler_1)
        {
            value = data.Puffer_Oben;
            offendingTemperature = "Puffer";
        }

        return BuildHeatingUpRequiredNotification(required, delta, value, threshold, offendingTemperature);
    }
}
