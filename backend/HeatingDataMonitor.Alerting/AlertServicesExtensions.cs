using HeatingDataMonitor.Alerting.Alerts;
using HeatingDataMonitor.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace HeatingDataMonitor.Alerting;

public static class AlertServicesExtensions
{
    public static IServiceCollection AddHeatingUpAlerts(this IServiceCollection services,
                                                        HeatingUpRequiredOptions options)
    {
        // Summer = only Boiler, but Winter = Boiler and Puffer, whichever is lower
        Func<HeatingData, float> valueGetter =
            options.SummerMode ? d => d.Boiler_1 : d => Math.Min(d.Boiler_1, d.Puffer_Oben);

        Duration timeThreshold = Duration.FromMinutes(options.MinutesBelowThreshold);

        // Recommendation: It's suggested to heat up. This does not repeat.
        services.AddSingleton<IAlert>(new FellBelowAlert(valueGetter, options.SuggestedThreshold, timeThreshold,
                                                         repeatAfter: null,
                                                         GetNotificationBuilder(required: false)));
        // Required: It's required/urgent to heat up. This one repeats.
        services.AddSingleton<IAlert>(new FellBelowAlert(valueGetter, options.RequiredThreshold, timeThreshold,
                                                         repeatAfter: Duration.FromHours(options.ReminderHours),
                                                         GetNotificationBuilder(required: true)));

        return services;

        FellBelowAlert.NotificationBuilder GetNotificationBuilder(bool required)
        {
            return (data, _, threshold, delta) =>
                NotificationBuilders.BuildHeatingUpRequiredNotification(required, delta, data, threshold,
                                                                        options.SummerMode);
        }
    }

    public static IServiceCollection AddAddFuelAlert(this IServiceCollection services, string configurationKey = "AddFuelAlert")
    {
        services.AddOptions<AddFuelOptions>()
                .BindConfiguration(configurationKey);
        services.AddSingleton<IAlert, AddFuelAlert>();

        return services;
    }
}
