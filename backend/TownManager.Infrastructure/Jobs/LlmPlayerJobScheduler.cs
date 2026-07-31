using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TownManager.Application.Llm;

namespace TownManager.Infrastructure.Jobs;

public class LlmPlayerJobScheduler(IServiceScopeFactory scopeFactory, IRecurringJobManager jobs,
    LlmPlayerConfig config, IHostApplicationLifetime appLifetime,
    ILogger<LlmPlayerJobScheduler> logger) : IHostedService
{
    public bool TickAtStart { get; set; }

    public Task StartAsync(CancellationToken ct)
    {
        if (TickAtStart)
        {
            appLifetime.ApplicationStarted.Register(() =>
            {
                _ = Task.Run(async () =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<Application.Players.Services.ILlmPlayerService>();
                    try { await service.ExecuteAsync(CancellationToken.None); }
                    catch (Exception ex) { logger.LogError(ex, "LLM player tick at startup failed"); }
                }, ct);
            });
        }

        Register(jobs, config);

        return Task.CompletedTask;
    }

    public static void Register(IRecurringJobManager jobs, LlmPlayerConfig config) =>
        jobs.AddOrUpdate<LlmPlayerJob>("llm-player-tick",
            j => j.ExecuteAsync(CancellationToken.None), config.TickIntervalCron);

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
