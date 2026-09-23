using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TownManager.Application.World;

namespace TownManager.Infrastructure.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 600)]
[AutomaticRetry(Attempts = 1)]
public class WorldResetJob(
    IWorldResetService resetService,
    WorldResetOptions options,
    IHostApplicationLifetime appLifetime,
    ILogger<WorldResetJob> logger)
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (options.IntervalDays <= 0)
        {
            logger.LogWarning("WorldReset:IntervalDays is {Days} — automatic reset disabled", options.IntervalDays);
            return;
        }

        var current = await resetService.GetCurrentIterationAsync(ct);
        if (current is null) return;

        var dueAt = WorldResetSchedule.DueAt(current.StartedAt, options.IntervalDays);
        if (DateTime.UtcNow < dueAt)
        {
            logger.LogDebug("World iteration {Iteration} ends at {DueAt:u} — no reset yet", current.Number, dueAt);
            return;
        }

        logger.LogWarning("World iteration {Iteration} (started {StartedAt:u}) is over — resetting",
            current.Number, current.StartedAt);

        var result = await resetService.ResetAsync(ct);
        logger.LogWarning("World reset done, iteration {Iteration} started — restarting app to re-seed",
            result.IterationNumber);

        appLifetime.StopApplication();
    }
}
