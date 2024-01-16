namespace HeatingDataMonitor.API.Alerting.Alerts;

public class HeatingUpRequiredOptions
{
    public int SuggestedThreshold { get; set; } = 40;
    public int RequiredThreshold { get; set; } = 30;
    public float MinutesBelowThreshold { get; set; } = 5;
    public float ReminderHours { get; set; } = 1;
}
