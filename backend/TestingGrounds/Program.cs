using System.Runtime.CompilerServices;
using HeatingDataMonitor.Database;
using Microsoft.Extensions.DependencyInjection;
using TestingGrounds;
/*
using CancellationTokenSource cts0 = new();
Task someTask = Task.Run(SomeMethod, cts0.Token);
cts0.CancelAfter(2000);

async Task SomeMethod()
{
    // await Task.Delay(10000, cts0.Token); --> is canceled, throws TaskCanceledException when awaited
    // throw new OperationCanceledException(); --> is canceled as well, throws that exception when awaited, even without cancellation stuff
    await Task.Delay(500);
    throw new InvalidOperationException("some exception");
}

await Task.Delay(3000);

Console.WriteLine($"Status of task: {someTask.Status}");
Console.WriteLine($"Is canceled: {someTask.IsCanceled}");
Console.WriteLine($"Is faulted: {someTask.IsFaulted}");
Console.WriteLine("awaiting task now..");
await someTask;

return;
*/

IServiceCollection services = new ServiceCollection();
services.AddHeatingDataDatabaseTimescaledb(
    "Server=127.0.0.1;Port=5432;Database=heating_data_monitor;User Id=heatingDataMonitorUser;Password=dontworrythispasswordwillchangeinproduction;Max Auto Prepare=10;Auto Prepare Min Usages=2;");
services.AddHeatingDataReceiverTimescaleDb();
services.AddTransient<PostgresNotificationStuff>();
services.AddLogging();

await using var sp = services.BuildServiceProvider();

using CancellationTokenSource cts = new();
/* GOD WHYYY. This somehow interfered with WaitAsync..
 I have no idea how but even if I don't pass any cancellation token to
 WaitAsync, cancelling with a Ctrl+C will cause WaitAsync to block indefinitely
Console.CancelKeyPress += (o, e) =>
{
    Console.WriteLine("Cancelled?");
    cts.Cancel();
};
*/

Task task = sp.GetRequiredService<PostgresNotificationStuff>().DoStuff(cts.Token);

Console.WriteLine("Press any key to cancel.");
Console.ReadKey(true);
cts.Cancel();
await task;

Console.WriteLine("Press any key to exit..");
Console.ReadKey(true);

