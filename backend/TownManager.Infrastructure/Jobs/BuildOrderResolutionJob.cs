using Hangfire;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs;

public class BuildOrderResolutionJob(IVillageRepository repo, AppDbContext db, ILogger<BuildOrderResolutionJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAndOrdersAsync(orderId, ct);
        if (village is null)
        {
            logger.LogWarning("Village not found for build order {OrderId}, skipping", orderId);
            return;
        }

        var order = village.BuildOrders.FirstOrDefault(o => o.Id == orderId);
        if (order is null)
        {
            logger.LogWarning("Build order {OrderId} not found, skipping", orderId);
            return;
        }

        var building = village.Buildings.First(b => b.Type == order.BuildingType);
        building.Level = order.TargetLevel;

        village.BuildOrders.Remove(order);

        await db.SaveChangesAsync(ct);
    }
}