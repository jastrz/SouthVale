using Hangfire;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs;

public class TrainOrderResolutionJob(
    IVillageRepository repo,
    AppDbContext db,
    IJobScheduler scheduler,
    ILogger<TrainOrderResolutionJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, CancellationToken ct)
    {
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
        
        var elapsed = DateTime.UtcNow - order.StartedAt;
        var shouldBeCompleted = (int)(elapsed / order.TimePerUnit);
        var newlyCompleted = Math.Min(shouldBeCompleted, order.Amount) - order.Completed;

        if (newlyCompleted > 0)
        {
            village.Troops = village.Troops.Add(order.Type, newlyCompleted);
            order.Completed += newlyCompleted;
        }

        if (order.Completed < order.Amount)
        {
            // reschedule for the next unit
            scheduler.ScheduleTrainOrderResolution(orderId, order.TimePerUnit);
        }
        else
        {
            village.TrainOrders.Remove(order);
        }
        
        await db.SaveChangesAsync(ct);
    }
}