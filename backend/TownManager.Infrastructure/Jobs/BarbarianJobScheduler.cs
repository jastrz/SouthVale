using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TownManager.Application.Villages.Services;
using TownManager.Domain.Config;

namespace TownManager.Infrastructure.Jobs;

public class BarbarianJobScheduler(IServiceScopeFactory scopeFactory, IRecurringJobManager jobs) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tick = scope.ServiceProvider.GetRequiredService<IBarbarianTickService>();
        await tick.ExecuteAsync(ct);

        jobs.AddOrUpdate<BarbarianTickJob>("barbarian-tick",
            j => j.ExecuteAsync(CancellationToken.None), BarbarianConfig.TickIntervalCron);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
