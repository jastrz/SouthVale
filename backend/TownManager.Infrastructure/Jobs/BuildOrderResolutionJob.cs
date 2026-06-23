using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs;

public class BuildOrderResolutionJob(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    AppDbContext db,
    IGameNotificationService notifications,
    ILogger<BuildOrderResolutionJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, PerformContext context, CancellationToken ct)
    {
        var currentJobId = context.BackgroundJob.Id;

        var village = await villageRepo.GetWithBuildingsAndOrdersAsync(orderId, ct);
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

        if (order.JobId != currentJobId)
        {
            logger.LogInformation(
                "Build order {OrderId} was rescheduled (current job {CurrentJobId} != stored {StoredJobId}), skipping",
                orderId, currentJobId, order.JobId);
            return;
        }

        var building = village.Buildings.First(b => b.Type == order.BuildingType);
        building.Level = order.TargetLevel;

        village.BuildOrders.Remove(order);

        await db.SaveChangesAsync(ct);

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
        if (userId is not null)
            await notifications.VillageUpdatedAsync(userId, village.Id, ct);
    }
}
