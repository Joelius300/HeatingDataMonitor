using System.Diagnostics;
using System.Runtime.CompilerServices;
using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Read;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestingGrounds;

IRealTimeConnectionManager connectionManager = new RealTimeConnectionManager();

connectionManager.FirstUserConnected += (o, e) => Console.WriteLine("First user connected.");
connectionManager.LastUserDisconnected += (o, e) => Console.WriteLine("Last user disconnected.");

connectionManager.UserConnected("abc");
Console.WriteLine("Connected Event should've been triggered.");
Debug.Assert(connectionManager.ConnectedCount == 1);

connectionManager.UserConnected("abc");
Debug.Assert(connectionManager.ConnectedCount == 1);
connectionManager.UserConnected("abcd");
Debug.Assert(connectionManager.ConnectedCount == 2);
connectionManager.UserConnected("abcdasf");
Debug.Assert(connectionManager.ConnectedCount == 3);
Console.WriteLine("Nothing should've been triggered until now.");

connectionManager.UserDisconnected("abcd");
Debug.Assert(connectionManager.ConnectedCount == 2);
connectionManager.UserDisconnected("abcd");
Debug.Assert(connectionManager.ConnectedCount == 2);
connectionManager.UserDisconnected("abcdasf");
Debug.Assert(connectionManager.ConnectedCount == 1);

Console.WriteLine("Nothing should've been triggered until now.");

connectionManager.UserDisconnected("abc");
Console.WriteLine("Disconnected Event should've been triggered.");
Debug.Assert(connectionManager.ConnectedCount == 0);

connectionManager.UserConnected("reee");
Console.WriteLine("connected should've been fired");
connectionManager.UserDisconnected("reee");
Console.WriteLine("disconnected should've been fired");

return;

IServiceCollection services = new ServiceCollection();
services.AddNpgsqlConnectionProvider(
    "Server=127.0.0.1;Port=5432;Database=heating_data_monitor;User Id=heatingDataMonitorUser;Password=dontworrythispasswordwillchangeinproduction;Max Auto Prepare=10;Auto Prepare Min Usages=2;");
services.AddHeatingDataTimescaledbReadonly();
services.AddTransient<PostgresNotificationStuff>();
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace));

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

// cts.CancelAfter(12000);
await sp.GetRequiredService<PostgresNotificationStuff>().DoStuff(cts.Token);

// GOD YOU FUCKING IDIOT
// you did it again, this obviously only throws exceptions once I cancel because that's when the task is awaited...
// stop doing that
// Console.WriteLine("Press any key to cancel.");
// Console.ReadKey(true);
// cts.Cancel();

Console.WriteLine("Press any key to exit..");
Console.ReadKey(true);

