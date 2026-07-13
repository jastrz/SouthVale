using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TownManager.Application.Barbarians;
using TownManager.Application.Villages.Services;

namespace TownManager.Infrastructure.Jobs;

public class BarbarianJobScheduler(IServiceScopeFactory scopeFactory, IRecurringJobManager jobs,
    IHostApplicationLifetime appLifetime, ILogger<BarbarianJobScheduler> logger,
    BarbarianOptions options) : IHostedService
{
    public bool TickAtStart { get; set; } = false;

    public Task StartAsync(CancellationToken ct)
    {
        if (TickAtStart)
        {
            appLifetime.ApplicationStarted.Register(() =>
            {
                _ = Task.Run(async () =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var tick = scope.ServiceProvider.GetRequiredService<IBarbarianTickService>();
                    try { await tick.ExecuteAsync(ct); }
                    catch (Exception ex) { logger.LogError(ex, "Barbarian tick at startup failed"); }
                }, ct);
            });
        }

        jobs.AddOrUpdate<BarbarianTickJob>("barbarian-tick",
            j => j.ExecuteAsync(ct), options.TickIntervalCron);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
