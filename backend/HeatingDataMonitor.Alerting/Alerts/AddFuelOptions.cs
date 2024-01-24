namespace HeatingDataMonitor.Alerting.Alerts;

public class AddFuelOptions
{
    /// Raise alarm when this exhaust temperature is reached.
    public float AddFuelTemperature { get; set; } = 125;
    /// Reset when this exhaust temperature is reached because it's too low to rekindle.
    public float LowerBound { get; set; } = 80;
    /// Exhaust temperature at which the heating unit is running close to peak.
    public float UpperBound { get; set; } = 180;
    /// How many minutes until the next notification is sent when no fuel is added.
    public float RepeatIntervalMinutes { get; set; } = 10;
    /// Number of degrees the exhaust temperature needs to rise for the alert to recognize it as "fuel was added".
    public float RequiredTemperatureDelta { get; set; } = 10;
    /// Number of times (heating cycles) the alert should be raised until reset.
    public int NumShouldAddFuel { get; set; } = 1;
}
