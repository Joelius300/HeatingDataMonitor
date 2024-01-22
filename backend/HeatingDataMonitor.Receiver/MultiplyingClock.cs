using NodaTime;

namespace HeatingDataMonitor.Receiver;

/// <summary>
/// A NodaTime clock that ticks faster by some factor.
/// </summary>
public class MultiplyingClock : IClock
{
    private readonly float _multiplier;

    private Instant? _current;
    private Instant _realTime;

    public MultiplyingClock(float multiplier) => _multiplier = multiplier;

    public Instant GetCurrentInstant()
    {
        Instant now = SystemClock.Instance.GetCurrentInstant();
        if (_current is null)
        {
            _current = now;
        }
        else
        {
            Duration delta = now - _realTime;
            _current += delta * _multiplier;
        }

        _realTime = now;
        return _current!.Value;
    }
}
