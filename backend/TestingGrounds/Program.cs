using System.Runtime.CompilerServices;
using HeatingDataMonitor.Database;
using Microsoft.Extensions.DependencyInjection;
using TestingGrounds;

async IAsyncEnumerable<int> GetEnumerable([EnumeratorCancellation] CancellationToken cancellationToken)
{
    try
    {
        int i = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return i++;
            try
            {
                await Task.Delay(500, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
    finally
    {
        Console.WriteLine("After loop");
    }
}


CancellationTokenSource ctsss = new();
Console.WriteLine("With break");
await foreach (int i in GetEnumerable(ctsss.Token))
{
    Console.WriteLine(i);
    if (i == 5)
        break;
}

Console.WriteLine("with cancel");
await foreach (int i in GetEnumerable(ctsss.Token))
{
    Console.WriteLine(i);
    if (i == 5)
        ctsss.Cancel();
}


class Enumerable : IAsyncEnumerable<int>, IAsyncDisposable
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) => new Enumerator();
    public async ValueTask DisposeAsync() => Console.WriteLine("Enumerable disposed");
}

class Enumerator : IAsyncEnumerator<int>
{
    public async ValueTask DisposeAsync() => Console.WriteLine("Enumerator disposed");

    public async ValueTask<bool> MoveNextAsync()
    {
        return Current++ <= 5;
    }

    public int Current { get; private set; }
}

return;



IServiceCollection services = new ServiceCollection();
services.AddHeatingDataDatabaseTimescaledb(
    "Server=127.0.0.1;Port=5432;Database=heating_data_monitor;User Id=heatingDataMonitorUser;Password=dontworrythispasswordwillchangeinproduction;Max Auto Prepare=10;Auto Prepare Min Usages=2;");
services.AddTransient<PostgresNotificationStuff>();
services.AddLogging();

await using var sp = services.BuildServiceProvider();

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (o, e) =>
{
    Console.WriteLine("Cancelled?");
    cts.Cancel();
};

await sp.GetRequiredService<PostgresNotificationStuff>().DoStuff(cts.Token);
