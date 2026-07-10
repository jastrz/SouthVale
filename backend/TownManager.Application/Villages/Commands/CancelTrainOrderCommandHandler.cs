using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Commands;

public class CancelTrainOrderCommandHandler(IVillageRepository repo, IJobScheduler scheduler, IPlayerRepository playerRepo)
    : IRequestHandler<CancelTrainOrderCommand, Result>
{
    public async Task<Result> Handle(CancelTrainOrderCommand request, CancellationToken ct)
    {
        var village = await repo.GetWithTrainOrdersAsync(request.OrderId, ct);
        if (village is null)
            return Result.Failure(["Train order not found."], statusCode: 404);

        var player = await playerRepo.GetByUserIdAsync(request.UserId, ct);
        if (player is null)
            return Result.Failure(["Player not found."], statusCode: 404);

        if (village.PlayerId != player.Id)
            return Result.Failure(["You do not own this village."], statusCode: 403);

        var order = village.TrainOrders.FirstOrDefault(o => o.Id == request.OrderId);
        if (order is null)
            return Result.Failure(["Train order not found."], statusCode: 404);

        var costPerUnit = TroopsConfig.Get(order.Type).TrainingCost;
        var uncompleted = order.Amount - order.Completed;
        village.Resources = village.Resources.Add(costPerUnit.Multiply(uncompleted));

        if (order.JobId is not null)
            scheduler.DeleteJob(order.JobId);

        village.TrainOrders.Remove(order);

        // Recalculate timing for remaining orders 
        var remaining = village.TrainOrders.OrderBy(o => o.StartsAt).ToList();
        for (var i = 0; i < remaining.Count; i++)
        {
            var remainingDuration = remaining[i].TimePerUnit * (remaining[i].Amount - remaining[i].Completed);
            remaining[i].StartsAt = i == 0 ? (remaining[i].StartsAt > DateTime.UtcNow ? DateTime.UtcNow : remaining[i].StartsAt ) : remaining[i - 1].CompletesAt;
            remaining[i].CompletesAt = remaining[i].StartsAt + remainingDuration;
            remaining[i].UpdatedAt = DateTime.UtcNow;

            var jobId = remaining[i].JobId;
            if (jobId is not null)
                scheduler.DeleteJob(jobId);
        }

        await repo.SaveChangesAsync(ct);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "cancel-train",
            new { Type = order.Type, CancelledUnits = uncompleted });

        for (int i = 0; i < remaining.Count; i++)
        {
            var firstUnitDelay = remaining[i].StartsAt - DateTime.UtcNow + remaining[i].TimePerUnit;
            remaining[i].JobId = scheduler.ScheduleTrainOrderResolution(remaining[i].Id, firstUnitDelay < TimeSpan.Zero ? TimeSpan.Zero : firstUnitDelay);
        }
        
        await repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}
