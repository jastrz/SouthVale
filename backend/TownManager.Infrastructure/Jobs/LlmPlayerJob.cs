using Hangfire;
using Microsoft.Extensions.Logging;
using TownManager.Application.Players.Services;
using TownManager.Domain.Config;

namespace TownManager.Infrastructure.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 900)]
[AutomaticRetry(Attempts = 1)]
public class LlmPlayerJob(ILlmPlayerService service, FeatureFlags features, ILogger<LlmPlayerJob> logger)
{
    public Task ExecuteAsync(CancellationToken ct)
    {
        if (!features.UseLlmPlayers)
        {
            logger.LogInformation("LLM players disabled — skipping tick");
            return Task.CompletedTask;
        }
        return service.ExecuteAsync(ct);
    }
}
