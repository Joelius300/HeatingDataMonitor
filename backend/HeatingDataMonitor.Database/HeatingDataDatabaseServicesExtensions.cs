using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HeatingDataMonitor.Database;

public static class HeatingDataDatabaseServicesExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="IConnectionProvider{TConnection}"/> for connection type
    /// <see cref="NpgsqlConnection"/> with pre-configured (static) defaults for Dapper &amp; NodaTime.
    /// Set 'Max Auto Prepare' to enable automatic statement preparation
    /// for better performance (see <a href="https://www.npgsql.org/doc/prepare.html#automatic-preparation">here</a>).
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">The connection string for the timescaledb database. Use Max Auto Prepare.</param>
    public static IServiceCollection AddNpgsqlConnectionProvider(this IServiceCollection services,
        string connectionString)
    {
        services.TryAddSingleton<IConnectionProvider<NpgsqlConnection>>(s =>
            new NpgsqlConnectionProvider(connectionString, s.GetRequiredService<ILogger<NpgsqlConnectionProvider>>()));

        return services;
    }
}
