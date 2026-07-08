using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TownManager.Domain.Config;

namespace TownManager.Infrastructure.Jobs;

public class LlmPlayerJobScheduler(IServiceScopeFactory scopeFactory, IRecurringJobManager jobs,
    LlmPlayerConfig config) : IHostedService
{
    public bool TickAtStart { get; set; }

    public async Task StartAsync(CancellationToken ct)
    {
        if (TickAtStart)
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<Application.Players.Services.ILlmPlayerService>();
            await service.ExecuteAsync(ct);
        }

        jobs.AddOrUpdate<LlmPlayerJob>("llm-player-tick",
            j => j.ExecuteAsync(CancellationToken.None), config.TickIntervalCron);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
