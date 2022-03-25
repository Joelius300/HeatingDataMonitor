namespace HeatingDataMonitor.Receiver;

public class DbResilienceOptions
{
    public const int DefaultExpectedNewRecordIntervalMilliseconds = 6000;

    public int RetryDurationMinutes { get; set; } = 10;
    public int RetryIntervalSeconds { get; set; } = 1;
    public int ExpectedNewRecordIntervalMilliseconds { get; set; } = DefaultExpectedNewRecordIntervalMilliseconds;
}
