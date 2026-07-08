using Hangfire;
using Serilog;
using Serilog.Events;
using TownManager.Api;
using TownManager.Api.Endpoints;
using TownManager.Application;
using TownManager.Application.Villages;
using TownManager.Infrastructure;

Directory.CreateDirectory("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Filter.ByExcluding(e =>
        e.Properties.TryGetValue("RequestPath", out var path) &&
        path.ToString().Contains("/hangfire"))
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("VillageActivity"))
        .WriteTo.File($"logs/village-activity-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .CreateLogger();

VillageActivity.Log = (playerId, villageName, action, details) =>
    Log.ForContext("VillageActivity", true)
       .Information("{Player} {Action} in {Village}: {@Details}", playerId, action, villageName, details);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AddApiServices(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    var app = builder.Build()
        .ConfigurePipeline();

    app.UseHangfireDashboard("/hangfire");

    app.MapEndpoints();

    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
