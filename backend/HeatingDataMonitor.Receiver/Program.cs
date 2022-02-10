using HeatingDataMonitor.Database;
using HeatingDataMonitor.Receiver;
using NodaTime;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // TODO make sure to use Type=notify in Systemd file
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddHeatingDataDatabaseTimescaledb(context.Configuration.GetConnectionString("HeatingDataDatabase"));
        services.AddSingleton<IHeatingDataReceiver, CsvHeatingDataReceiver>();
        services.AddHostedService<DbInsertionService>();

        string? path = context.Configuration.GetValue<string?>("FakeSerialPortData");
        if (!string.IsNullOrEmpty(path))
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Specified csv file not found", path);

            services.AddSingleton<ICsvHeatingDataReader>(new FileCsvHeatingDataReader(path));
        }
        else
        {
            services.AddOptions<SerialPortOptions>()
                    .BindConfiguration("Serial");
            services.AddSingleton<ICsvHeatingDataReader, SerialPortCsvHeatingDataReader>();
        }
    })
    .Build();

await host.RunAsync();
