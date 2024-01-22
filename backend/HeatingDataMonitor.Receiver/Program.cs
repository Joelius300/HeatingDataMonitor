using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Write;
using HeatingDataMonitor.Receiver;
using HeatingDataMonitor.Receiver.Shared;
using NodaTime;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((context, services) =>
    {
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

            int expectedInterval = context.Configuration.GetValue("DbResilience:ExpectedNewRecordIntervalMilliseconds",
                                                                  DbResilienceOptions
                                                                      .DefaultExpectedNewRecordIntervalMilliseconds);
            int delay = context.Configuration.GetValue("FakeSerialPort:NewRecordInterval", expectedInterval);
            services.AddSingleton<ICsvHeatingDataReader>(new FileCsvHeatingDataReader(path, delay));
            float clockMultiplier = (float)expectedInterval / delay;
            // speed everything up if FakeSerialPort has a faster interval than the expected interval
            services.AddSingleton<IClock>(new MultiplyingClock(clockMultiplier));
        }
        else
        {
            services.AddOptions<SerialPortOptions>()
                    .BindConfiguration("Serial");
            services.AddSingleton<ICsvHeatingDataReader, SerialPortCsvHeatingDataReader>();
            services.AddSingleton<IClock>(SystemClock.Instance);
        }
    })
    .Build();

await host.RunAsync();
