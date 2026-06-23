using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs;

public class TrainOrderResolutionJob(
    IVillageRepository repo,
    IPlayerRepository playerRepo,
    AppDbContext db,
    IJobScheduler scheduler,
    IGameNotificationService notifications,
    ILogger<TrainOrderResolutionJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, PerformContext context, CancellationToken ct)
    {
        var currentJobId = context.BackgroundJob.Id;

        var village = await repo.GetWithTrainOrdersAsync(orderId, ct);
        if (village is null)
        {
            logger.LogWarning("Village not found for train order {OrderId}, skipping", orderId);
            return;
        }

        var order = village.TrainOrders.FirstOrDefault(o => o.Id == orderId);
        if (order is null)
        {
            logger.LogWarning("Train order {OrderId} not found, skipping", orderId);
            return;
        }

        if (order.JobId != currentJobId)
        {
            logger.LogInformation(
                "Train order {OrderId} was rescheduled (current job {CurrentJobId} != stored {StoredJobId}), skipping",
                orderId, currentJobId, order.JobId);
            return;
        }

        var elapsed = DateTime.UtcNow - order.StartsAt;
        var shouldBeCompleted = (int)(elapsed / order.TimePerUnit);
        var newlyCompleted = Math.Min(shouldBeCompleted, order.Amount) - order.Completed;

        if (newlyCompleted > 0)
        {
            village.Troops = village.Troops.Add(order.Type, newlyCompleted);
            order.Completed += newlyCompleted;
        }
        
        await db.SaveChangesAsync(ct);
        
        if (order.Completed < order.Amount)
        {
            // reschedule for the next unit
            var jobId = scheduler.ScheduleTrainOrderResolution(orderId, order.TimePerUnit);
            order.JobId = jobId;
        }
        else
        {
            village.TrainOrders.Remove(order);
        }
        
        await db.SaveChangesAsync(ct);

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
        if (userId is not null)
            await notifications.VillageUpdatedAsync(userId, village.Id, ct);
    }
}
