using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataHandler;
using DataHandler.Services;
using RaspberryPIUtils;
using DataHistory;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO.Ports;
using System.IO;

namespace WebCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<RaspberryPI>();
            services.AddSingleton<DataStorage>();

            services
                .AddOptions<HeatingMonitorOptions>()
                .Bind(Configuration.GetSection("HeatingMonitor"))
                .ValidateDataAnnotations()
                .Validate(op =>
                {
                    if (op.DebugMode)
                    {
                        // In debug-mode you can use a File-Path as SP-name which will be processed as messages (line = message) from the SPS (eng. PLC)
                        return File.Exists(op.SerialPortName);
                    }
                    else
                    {
                        return SerialPort.GetPortNames().Contains(op.SerialPortName);
                    }
                });

            IConfigurationSection historyConfig = Configuration.GetSection("HistoryService");
            services
                .AddOptions<HistoryServiceOptions>()
                .Bind(historyConfig)
                .ValidateDataAnnotations();

            services.AddDbContextPool<HeatingDataContext>(optionsBuilder =>
                optionsBuilder.UseSqlite(historyConfig.GetValue<string>("SQLiteConnectionString")));

            services.AddScoped<IHistoryRepository, HistoryRepository>();

            if (Configuration.GetValue<bool>("HeatingMonitor:DebugMode", false))
            {
                services.AddHostedService<MockDataService>();
            }
            else
            {
                services.AddHostedService<SerialDataService>();
                services.AddHostedService<HistoryService>(); // data should only be archived if it comes from the serial port
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
