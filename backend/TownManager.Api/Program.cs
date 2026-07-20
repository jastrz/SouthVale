using Hangfire;
using Serilog;
using Serilog.Events;
using TownManager.Api;
using TownManager.Api.Endpoints;
using TownManager.Application;
using TownManager.Application.Players;
using TownManager.Application.Villages;
using TownManager.Application.Llm;
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
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(e => e.Properties.ContainsKey("LlmPrompt") || e.Properties.ContainsKey("LlmActivity"))
        .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("VillageActivity"))
        .WriteTo.File($"logs/village-activity-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("LlmActivity"))
        .WriteTo.File($"logs/llm-activity-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("LlmPrompt"))
        .WriteTo.File($"logs/llm-prompt-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .CreateLogger();

VillageActivity.Log = (playerId, villageName, action, details) =>
    Log.ForContext("VillageActivity", true)
       .Information("{Player} {Action} in {Village}: {@Details}", playerId, action, villageName, details);

LlmActivity.Log = (username, action, model, details) =>
    Log.ForContext("LlmActivity", true)
       .Information("{Bot} [{Model}] {Action}: {@Details}", username, model, action, details);

LlmActivity.PromptLog = (username, prompt) =>
    Log.ForContext("LlmPrompt", true)
       .Information("{Bot} prompt:\n{Prompt}", username, prompt);

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AddApiServices(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    builder.Services.Configure<ServiceProviderOptions>(o => o.ValidateOnBuild = false);

    var app = builder.Build()
        .ConfigurePipeline();

    if (app.Environment.IsDevelopment())
        app.UseHangfireDashboard("/hangfire");

    app.MapEndpoints();

    var llmCfg = app.Services.GetRequiredService<LlmPlayerConfig>();
    Log.Information("LLM config: model={Model}, api={Api}",
        llmCfg.Model, llmCfg.ApiUrl,
        string.IsNullOrEmpty(llmCfg.ApiKey) ? "not set" : "set");

    if (!string.IsNullOrEmpty(llmCfg.ApiKey))
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new("Bearer", llmCfg.ApiKey);
            var res = await http.GetAsync($"{llmCfg.ApiUrl.TrimEnd('/')}/models", CancellationToken.None);
            if (res.IsSuccessStatusCode)
                Log.Information("LLM API connected, models endpoint OK");
            else
                Log.Warning("LLM API returned {Status} — check key and URL", res.StatusCode);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "LLM API unreachable — check URL and network");
        }
    }

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
