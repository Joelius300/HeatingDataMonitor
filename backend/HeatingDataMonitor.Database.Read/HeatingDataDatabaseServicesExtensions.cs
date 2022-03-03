using HeatingDataMonitor.Receiver.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace HeatingDataMonitor.Database.Read;

public static class HeatingDataDatabaseServicesExtensions
{
    /// <summary>
    /// Registers singleton <see cref="IHeatingDataRepository"/> and <see cref="IHeatingDataReceiver"/> dependencies
    /// which are connected to a timescaledb database through a <c>IConnectionProvider&lt;NpgsqlConnection&gt;</c>
    /// (<see cref="IConnectionProvider{TConnection}"/> / <see cref="NpgsqlConnection"/>) dependency.
    /// </summary>
    /// <param name="services"></param>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddHeatingDataTimescaledbReadonly(this IServiceCollection services)
    {
        services.TryAddSingleton<IHeatingDataRepository, TimescaledbHeatingDataRepository>();
        services.TryAddSingleton<IHeatingDataReceiver, NpgsqlNotificationHeatingDataReceiver>();

        return services;
    }
}
