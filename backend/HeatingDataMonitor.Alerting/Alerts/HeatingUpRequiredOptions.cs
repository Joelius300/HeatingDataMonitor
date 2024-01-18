namespace HeatingDataMonitor.Alerting.Alerts;

public class HeatingUpRequiredOptions
{
    /// Whether the heating unit is operating in summer mode and Puffer should not be taken into account.
    // Unfortunately, there is no way to determine this from the data directly so it has to be updated.
    // Since this only changes twice a year, it's fine to quickly update the value and restart the container.
    // But for a more sophisticated setup, this could be configurable via the user interface.
    // Also, an AI model or algorithm would most likely be able to find the point easily if trained correctly.
    public bool SummerMode { get; set; }

    public int SuggestedThreshold { get; set; } = 40;
    public int RequiredThreshold { get; set; } = 30;
    public float MinutesBelowThreshold { get; set; } = 5;
    public float ReminderHours { get; set; } = 1;
}
