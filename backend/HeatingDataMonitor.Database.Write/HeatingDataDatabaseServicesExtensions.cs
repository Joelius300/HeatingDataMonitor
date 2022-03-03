using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace HeatingDataMonitor.Database.Write;

public static class HeatingDataDatabaseServicesExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="IHeatingDataWriteRepository"/> dependency
    /// which is connected to a timescaledb database through a <c>IConnectionProvider&lt;NpgsqlConnection&gt;</c>
    /// (<see cref="IConnectionProvider{TConnection}"/> / <see cref="NpgsqlConnection"/>) dependency.
    /// </summary>
    /// <param name="services"></param>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddHeatingDataWriteRepositoryTimescaledb(this IServiceCollection services)
    {
        services.TryAddSingleton<IHeatingDataWriteRepository, TimescaledbHeatingDataWriteRepository>();

        return services;
    }
}
