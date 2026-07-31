using Hangfire;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
public class VillageTickJob(IVillageTickService service, ILogger<VillageTickJob> logger)
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            await service.ExecuteAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Village tick failed");
            throw;
        }
    }
}
