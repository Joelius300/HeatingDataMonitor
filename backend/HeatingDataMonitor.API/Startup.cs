using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HeatingDataMonitor.API.Hubs;
using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.History;
using HeatingDataMonitor.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime.Serialization.SystemTextJson;

namespace HeatingDataMonitor.API
{
    public class Startup
    {
        private const string DebugPolicyName = "DebugPolicy";
        private readonly Action<JsonSerializerOptions> _configureJsonOptions = options =>
        {
            // We expect a lot of null values so let's not blow up the response unnecessarily.
            // Also we want to keep the original names.
            options.IgnoreNullValues = true;
            options.PropertyNamingPolicy = null;
            options.Converters.Add(NodaConverters.InstantConverter);
            options.Converters.Add(NodaConverters.LocalDateTimeConverter);
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddJsonOptions(options => _configureJsonOptions(options.JsonSerializerOptions));
            
            services.AddDbContext<HeatingDataDbContext>(builder =>
                    builder.UseNpgsql(Configuration.GetConnectionString("HeatingDataDatabase"),
                                      options => options.UseNodaTime())
                           // We only add and fetch rows so we don't need to waste performance on tracking.
                           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddSingleton(typeof(IJsonStreamingResultExecutor<>), typeof(JsonStreamingResultExecutor<>));

            services.AddSignalR()
                    .AddJsonProtocol(options => _configureJsonOptions(options.PayloadSerializerOptions));

            services.AddCors(options =>
            {
                options.AddPolicy(DebugPolicyName, builder => builder
                                                   .AllowAnyMethod()
                                                   .AllowAnyHeader()
                                                   .AllowCredentials()
                                                   // only for the angular development server
                                                   .WithOrigins("http://localhost:4200"));
            });

            services.AddOptions<CacheOptions>()
                    .Bind(Configuration.GetSection("Cache"));

            IConfiguration serialSection = Configuration.GetSection("Serial");
            services.AddOptions<SerialHeatingDataOptions>()
                    .Bind(serialSection);

            // this and the FakeReceiver could probably be improved regarding encapsulation and responsibility
            string portName = serialSection.GetValue(nameof(SerialHeatingDataOptions.PortName), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sampleData.csv"));
            if (Path.GetExtension(portName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IHeatingDataReceiver, MockHeatingDataReceiver>();
                services.AddHostedService(sp => sp.GetRequiredService<IHeatingDataReceiver>());
            }
            else
            {
                services.AddSerialPortHeatingDataReceiver();
                services.AddHostedService<HeatingDataHistoryService>();
            }

            services.AddSingleton<HeatingDataCacheService>();
            services.AddHostedService(sp => sp.GetRequiredService<HeatingDataCacheService>());

            services.AddHostedService<HeatingDataRealTimeService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();

            // During development we want to be able to use the angular dev server
            if (env.IsDevelopment())
            {
                app.UseCors(DebugPolicyName);
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<HeatingDataHub>("/realTimeFeed");
            });
        }
    }
}
