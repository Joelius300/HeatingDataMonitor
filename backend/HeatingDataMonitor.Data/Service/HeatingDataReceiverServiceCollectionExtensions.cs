using HeatingDataMonitor.Service;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    // Inspired by the ..ServiceCollectionExtensions in the aspnetcore middlewares
    public static class HeatingDataReceiverServiceCollectionExtensions
    {
        public static IServiceCollection AddSerialPortHeatingDataReceiver(this IServiceCollection services)
        {
            services.AddOptions(); // add options if not done yet

            // If IConfigureOptions<SerialPortOptions> was already registered, this doesn't do anything.
            services.TryAddTransient<IConfigureOptions<SerialPortOptions>, SerialPortOptionsSetup>();

            services.AddSingleton<IHeatingDataReceiver, SerialPortHeatingDataReceiver>();
            services.AddHostedService(sp => sp.GetRequiredService<IHeatingDataReceiver>());

            return services;
        }
    }
}
