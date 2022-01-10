using System.Text.Json;
using System.Text.Json.Serialization;
using HeatingDataMonitor.API.Hubs;
using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Data.History;
using HeatingDataMonitor.Data.Service;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

void ConfigureOptions(JsonSerializerOptions options)
{
    // We expect a lot of null values so let's not blow up the response unnecessarily.
    // Also we want to keep the original names.
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.PropertyNamingPolicy = null;
    options.Converters.Add(NodaConverters.InstantConverter);
    options.Converters.Add(NodaConverters.LocalDateTimeConverter);
}

services.AddControllers()
        .AddJsonOptions(o => ConfigureOptions(o.JsonSerializerOptions));

services.AddDbContext<HeatingDataDbContext>(b =>
        b.UseNpgsql(configuration.GetConnectionString("HeatingDataDatabase"),
                          options => options.UseNodaTime())
               // We only add and fetch rows so we don't need to waste performance on tracking.
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

services.AddSignalR()
        .AddJsonProtocol(options => ConfigureOptions(options.PayloadSerializerOptions));

const string debugPolicyName = "DebugPolicy";
services.AddCors(options =>
{
    options.AddPolicy(debugPolicyName, b => b
                                       .AllowAnyMethod()
                                       .AllowAnyHeader()
                                       .AllowCredentials()
                                       // only for the angular development server
                                       .WithOrigins("http://localhost:4200"));
});

services.AddOptions<CacheOptions>()
        .Bind(configuration.GetSection("Cache"));

IConfiguration serialSection = configuration.GetSection("Serial");
services.AddOptions<SerialHeatingDataOptions>()
        .Bind(serialSection);

services.AddSingleton<IClock>(SystemClock.Instance);

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRouting();

// During development we want to be able to use the angular dev server
if (app.Environment.IsDevelopment())
{
    app.UseCors(debugPolicyName);
}

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<HeatingDataHub>("/realTimeFeed");
});

app.Run();
