using TestingGrounds;

const int count = 5;
var reader = new ReaderExample();

Console.WriteLine("--- EnumerateSomething, cancel with break");

int c = 0;
await foreach (string s in reader.EnumerateSomething())
{
    Console.WriteLine($"New string from enumerable: {s}");
    if (++c % count == 0)
    {
        Console.WriteLine("Manually cancelling now using break!");
        break;
    }
}

Console.WriteLine("--- EnumerateSomething, cancel after move next was called (inside body)");

CancellationTokenSource cts = new();
await foreach (string s in reader.EnumerateSomething().WithCancellation(cts.Token))
{
    Console.WriteLine($"New string from enumerable: {s}");
    if (++c % count == 0)
    {
        Console.WriteLine("Manually cancelling now using cts.Cancel()!");
        cts.Cancel();
    }
}
cts.Dispose();

Console.WriteLine("--- EnumerateSomething, cancel with token after 150ms");

cts = new();
cts.CancelAfter(TimeSpan.FromMilliseconds(150));
await foreach (string s in reader.EnumerateSomething().WithCancellation(cts.Token))
{
    Console.WriteLine($"New string from enumerable: {s}");
}
cts.Dispose();

Console.WriteLine("--- EnumerateSomething, cancel with token after 150ms AND break after second iteration (which should be after it was cancelled)");

int o = 0;
cts = new();
cts.CancelAfter(TimeSpan.FromMilliseconds(150));
await foreach (string s in reader.EnumerateSomething().WithCancellation(cts.Token))
{
    Console.WriteLine($"New string from enumerable: {s}");
    if (++o >= 2)
    {
        // it should never reach this because the second call to MoveNextAsync is cancelled, and thus should return false, meaning only one iteration is performed
        Console.WriteLine("Breaking after second iteration (actually second call to MoveNextAsync)");
        break;
    }
}
cts.Dispose();

Console.WriteLine("--- EnumerateSomething, store and enumerate twice (UNDEFINED PER GUIDELINES; WORKS IN OUR EXAMPLE)");

IAsyncEnumerable<string> enumerable = reader.EnumerateSomething();

Console.WriteLine("First iteration:");
await foreach (string s in enumerable)
{
    Console.WriteLine($"New string from enumerable: {s}");
    if (++c % count == 0)
    {
        Console.WriteLine("Manually cancelling now using break.");
        break;
    }
}

Console.WriteLine("Second iteration:");
await foreach (string s in enumerable)
{
    Console.WriteLine($"New string from enumerable: {s}");
    if (++c % count == 0)
    {
        Console.WriteLine("Manually cancelling now using break.");
        break;
    }
}
