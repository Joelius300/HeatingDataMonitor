using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace HeatingDataMonitor.Notifications;

public static class SignalServicesExtensions
{
    /// <summary>
    /// Adds a notification provider for Signal that communicates with a `signal-cli-rest-api` instance.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseUrl">Base url of the Rest API.</param>
    /// <param name="configurationKey">The configuration key for the registered options <see cref="SignalNotificationOptions"/>.</param>
    public static IServiceCollection AddSignalNotificationProvider(this IServiceCollection services, string baseUrl,
                                                                   string configurationKey = "SignalNotifications")
    {
        services.AddRefitClient<ISignalRestClient>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUrl));

        services.AddOptions<SignalNotificationOptions>()
                .BindConfiguration(configurationKey);
        services.AddSingleton<INotificationProvider, SignalRestNotificationProvider>();

        return services;
    }
}
