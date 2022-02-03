using System.Text.Json;
using System.Text.Json.Serialization;
using HeatingDataMonitor.API.Hubs;
using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Database;
using Microsoft.AspNetCore.HttpOverrides;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

void ConfigureJsonOptions(JsonSerializerOptions options)
{
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.PropertyNamingPolicy = null; // keep PascalCase naming
    options.Converters.Add(NodaConverters.InstantConverter);
    options.Converters.Add(NodaConverters.LocalDateTimeConverter);
}

services.AddHeatingDataDatabaseTimescaledb(configuration.GetConnectionString("HeatingDataDatabase"));

services.AddControllers()
        .AddJsonOptions(options => ConfigureJsonOptions(options.JsonSerializerOptions));

services.AddSignalR()
        .AddJsonProtocol(options => ConfigureJsonOptions(options.PayloadSerializerOptions));

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

// TODO once every reading is written in the db and real-time is moved to postgres notify, this caching stuff can be nuked
services.AddSingleton<HeatingDataCacheService>();
services.AddHostedService(sp => sp.GetRequiredService<HeatingDataCacheService>());

services.AddHostedService<HeatingDataRealTimeService>();

var app = builder.Build();

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

await app.RunAsync();
