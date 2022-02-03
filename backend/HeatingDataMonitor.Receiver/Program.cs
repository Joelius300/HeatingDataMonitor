using HeatingDataMonitor.Database;
using HeatingDataMonitor.Receiver;
using NodaTime;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // TODO make sure to use Type=notify in Systemd file
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SerialPortOptions>()
                .BindConfiguration("Serial");

        services.AddHeatingDataDatabaseTimescaledb(context.Configuration.GetConnectionString("HeatingDataDatabase"));
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IHeatingDataReceiver, SerialPortHeatingDataReceiver>();
        services.AddHostedService(sp => sp.GetRequiredService<IHeatingDataReceiver>());

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
