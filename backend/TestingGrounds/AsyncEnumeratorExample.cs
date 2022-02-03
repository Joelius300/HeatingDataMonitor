namespace TestingGrounds;

public class ReaderExample : IAsyncEnumerable<string>
{
    // Infinite iteration
    // Use await foreach and break to cancel
    // Supports WithCancellation
    public IAsyncEnumerable<string> EnumerateSomething() => this;

    IAsyncEnumerator<string> IAsyncEnumerable<string>.GetAsyncEnumerator(CancellationToken cancellationToken) => new AsyncEnumeratorExample(cancellationToken);
}

internal class AsyncEnumeratorExample : IAsyncEnumerator<string>
{
    private readonly CancellationToken _cancellationToken;

    public string Current { get; private set; } = null!;

    public AsyncEnumeratorExample(CancellationToken cancellationToken)
    {
        Console.WriteLine("Enumeration started; resources allocated.");
        _cancellationToken = cancellationToken;
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("Enumerator disposed; resources deallocated");

        return default;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("MoveNextAsync called after cancelled. Returning false.");
            return false;
        }

        try
        {
            await Task.Delay(100, _cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("\"long\" running operation inside MoveNextAsync cancelled. Returning false.");
            return false;
        }

        Current = new Random().NextInt64().ToString();
        return true;
    }
}
