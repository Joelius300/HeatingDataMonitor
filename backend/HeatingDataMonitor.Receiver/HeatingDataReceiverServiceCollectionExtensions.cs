using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HeatingDataMonitor.Receiver;

// Inspired by the ..ServiceCollectionExtensions in the aspnetcore middlewares
public static class HeatingDataReceiverServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddSerialPortHeatingDataReceiver(this IServiceCollection services)
    {
        services.AddOptions(); // add options if not done yet

        // If IConfigureOptions<SerialPortOptions> was already registered, this doesn't do anything.
        // If no options were configured, this setup instantiates a default config with the first
        // serial port found on this machine.
        services.TryAddTransient<IConfigureOptions<SerialHeatingDataOptions>, SerialHeatingDataOptionsSetup>();

        services.AddSingleton<IHeatingDataReceiver, SerialPortHeatingDataReceiver>();
        services.AddHostedService(sp => sp.GetRequiredService<IHeatingDataReceiver>());

        return services;
    }
}
