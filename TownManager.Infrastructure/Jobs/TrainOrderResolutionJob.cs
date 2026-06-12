using Hangfire;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

public class TrainOrderResolutionJob(IVillageRepository repo, IUnitOfWork uow, IJobScheduler scheduler)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, CancellationToken ct)
    {
        var village = await repo.GetWithTrainOrdersAsync(orderId, ct);
        if (village is null) return;

        var order = village.TrainOrders.FirstOrDefault(o => o.Id == orderId);
        if (order is null) return;
        
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
        
        await uow.SaveChangesAsync(ct);
    }
}