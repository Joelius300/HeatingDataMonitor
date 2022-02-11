using HeatingDataMonitor.Receiver.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HeatingDataMonitor.Database;

public static class HeatingDataDatabaseServicesExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="IHeatingDataRepository"/> dependency which is connected to
    /// a timescaledb database with the <paramref name="connectionString"/> (unless it was already registered).
    /// Set 'Max Auto Prepare' to enable automatic statement preparation for better performance
    /// (see <a href="https://www.npgsql.org/doc/prepare.html#automatic-preparation">here</a>).
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">The connection string for the timescaledb database. Use Max Auto Prepare.</param>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddHeatingDataDatabaseTimescaledb(this IServiceCollection services,
        string connectionString)
    {
        services.TryAddSingleton<IConnectionProvider<NpgsqlConnection>>(s =>
            new NpgsqlConnectionProvider(connectionString, s.GetRequiredService<ILogger<NpgsqlConnectionProvider>>()));
        services.TryAddSingleton<IHeatingDataRepository, TimescaledbHeatingDataRepository>();

        return services;
    }

    // TODO Do some splitting with read vs write related db stuff. One repo for writing (doesn't need access to views),
    // one for reading. Then the registration for reading will also register the receiver.
    public static IServiceCollection AddHeatingDataReceiverTimescaleDb(this IServiceCollection services)
    {
        services.TryAddSingleton<IHeatingDataReceiver, NpgsqlNotificationHeatingDataReceiver>();

        return services;
    }
}
