using System.Diagnostics;
using System.Runtime.CompilerServices;
using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Read;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TestingGrounds;

var logMock = new Mock<ILogger<RealTimeConnectionManager>>();
IRealTimeConnectionManager connectionManager = new RealTimeConnectionManager(logMock.Object);

connectionManager.FirstClientConnected += (o, e) => Console.WriteLine("First user connected.");
connectionManager.LastClientDisconnected += (o, e) => Console.WriteLine("Last user disconnected.");

connectionManager.ClientConnected("abc");
Console.WriteLine("Connected Event should've been triggered.");
Debug.Assert(connectionManager.ConnectedCount == 1);

connectionManager.ClientConnected("abc");
Debug.Assert(connectionManager.ConnectedCount == 1);
connectionManager.ClientConnected("abcd");
Debug.Assert(connectionManager.ConnectedCount == 2);
connectionManager.ClientConnected("abcdasf");
Debug.Assert(connectionManager.ConnectedCount == 3);
Console.WriteLine("Nothing should've been triggered until now.");

connectionManager.ClientDisconnected("abcd");
Debug.Assert(connectionManager.ConnectedCount == 2);
connectionManager.ClientDisconnected("abcd");
Debug.Assert(connectionManager.ConnectedCount == 2);
connectionManager.ClientDisconnected("abcdasf");
Debug.Assert(connectionManager.ConnectedCount == 1);

Console.WriteLine("Nothing should've been triggered until now.");

connectionManager.ClientDisconnected("abc");
Console.WriteLine("Disconnected Event should've been triggered.");
Debug.Assert(connectionManager.ConnectedCount == 0);

connectionManager.ClientConnected("reee");
Console.WriteLine("connected should've been fired");
connectionManager.ClientDisconnected("reee");
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

