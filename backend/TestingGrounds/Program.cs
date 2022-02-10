using HeatingDataMonitor.Database;
using Microsoft.Extensions.DependencyInjection;
using TestingGrounds;

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
