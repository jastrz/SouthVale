using Hangfire;
using Serilog;
using TownManager.Api;
using TownManager.Api.Endpoints;
using TownManager.Application;
using TownManager.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Filter.ByExcluding(e =>
        e.Properties.TryGetValue("RequestPath", out var path) &&
        path.ToString().Contains("/hangfire"))
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

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
