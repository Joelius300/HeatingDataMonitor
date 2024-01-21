using System.Text.Json;
using System.Text.Json.Serialization;
using HeatingDataMonitor.Alerting;
using HeatingDataMonitor.Alerting.Alerts;
using HeatingDataMonitor.API.Hubs;
using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Read;
using Microsoft.AspNetCore.HttpOverrides;
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

services.AddNpgsqlConnectionProvider(configuration.GetConnectionString("HeatingDataDatabase"));
services.AddHeatingDataTimescaledbReadonly();

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

services.AddSingleton<IRealTimeConnectionManager, RealTimeConnectionManager>();
services.AddHostedService<HeatingDataRealTimeService>();

services.AddHeatingUpAlerts(configuration.GetSection("HeatingUpAlert").Get<HeatingUpRequiredOptions>());
services.AddHostedService<AlertMonitor>();

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
