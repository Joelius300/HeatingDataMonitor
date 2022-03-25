using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Write;
using HeatingDataMonitor.Receiver;
using HeatingDataMonitor.Receiver.Shared;
using NodaTime;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // TODO make sure to use Type=notify in Systemd file
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddNpgsqlConnectionProvider(context.Configuration.GetConnectionString("HeatingDataDatabase"));
        services.AddHeatingDataWriteRepositoryTimescaledb();
        services.AddSingleton<IHeatingDataReceiver, CsvHeatingDataReceiver>();
        services.AddHostedService<DbInsertionService>();
        services.AddOptions<DbResilienceOptions>()
                .BindConfiguration("DbResilience");

        string? path = context.Configuration.GetValue<string?>("FakeSerialPort:CsvFilePath");
        if (!string.IsNullOrEmpty(path))
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Specified csv file not found", path);

            int delay = context.Configuration.GetValue("FakeSerialPort:NewRecordInterval",
                context.Configuration.GetValue("DbResilience:ExpectedNewRecordIntervalMilliseconds",
                    DbResilienceOptions.DefaultExpectedNewRecordIntervalMilliseconds));
            services.AddSingleton<ICsvHeatingDataReader>(new FileCsvHeatingDataReader(path, delay));
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
