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

            services.Configure<HeatingMonitorOptions>(Configuration.GetSection("HeatingMonitor"));
            services
                .AddOptions<HeatingMonitorOptions>()
                .ValidateDataAnnotations()
                .Validate(op => op.DebugMode || SerialPort.GetPortNames().Contains(op.SerialPortName));

            var historyConfig = Configuration.GetSection("HistoryService");
            if (historyConfig.Exists())
            {
                services.Configure<HistoryServiceOptions>(historyConfig);
                services.AddOptions<HistoryServiceOptions>().ValidateDataAnnotations();

                services.AddDbContextPool<HeatingDataContext>(optionsBuilder =>
                    optionsBuilder.UseSqlite(historyConfig.GetValue<string>("SQLiteConnectionString")));

                services.AddHostedService<HistoryService>();
                services.AddScoped<IHistoryRepository, HistoryRepository>();
            }

            if (Configuration.GetValue<bool>("HeatingMonitor:DebugMode", false))
            {
                services.AddHostedService<MockDataService>();
            }
            else
            {
                services.AddHostedService<SerialDataService>();
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
