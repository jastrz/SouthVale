using Hangfire;
using Microsoft.Extensions.Logging;
using TownManager.Application.Villages.Services;
using TownManager.Domain.Config;

namespace TownManager.Infrastructure.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 120)]
[AutomaticRetry(Attempts = 1)] 
public class BarbarianTickJob(IBarbarianTickService service, FeatureFlags features, ILogger<BarbarianTickJob> logger)
{
    public Task ExecuteAsync(CancellationToken ct)
    {
        if (!features.UseBarbarians)
        {
            logger.LogInformation("Barbarians disabled — skipping tick");
            return Task.CompletedTask;
        }
        return service.ExecuteAsync(ct);
    }
}
