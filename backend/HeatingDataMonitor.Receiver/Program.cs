using HeatingDataMonitor.Database;
using HeatingDataMonitor.Receiver;
using NodaTime;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // TODO make sure to use Type=notify in Systemd file
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SerialPortOptions>()
                .BindConfiguration("Serial");

        services.AddSingleton<IClock>(SystemClock.Instance);
        // services.AddSingleton<ICsvHeatingDataReader, SerialPortCsvHeatingDataReader>();
        services.AddSingleton<ICsvHeatingDataReader>(new FileCsvHeatingDataReader("/home/joel/Desktop/output.csv", 10));
        services.AddSingleton<IHeatingDataReceiver, CsvHeatingDataReceiver>();
        // services.AddHeatingDataDatabaseTimescaledb(context.Configuration.GetConnectionString("HeatingDataDatabase"));

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
