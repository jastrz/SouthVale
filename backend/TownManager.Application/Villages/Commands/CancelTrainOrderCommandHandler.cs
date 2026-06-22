using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Commands;

public class CancelTrainOrderCommandHandler(IVillageRepository repo, IJobScheduler scheduler)
    : IRequestHandler<CancelTrainOrderCommand, Result>
{
    public async Task<Result> Handle(CancelTrainOrderCommand request, CancellationToken ct)
    {
        var village = await repo.GetWithTrainOrdersAsync(request.OrderId, ct);
        if (village is null)
            return Result.Failure(["Train order not found."], statusCode: 404);

        var order = village.TrainOrders.FirstOrDefault(o => o.Id == request.OrderId);
        if (order is null)
            return Result.Failure(["Train order not found."], statusCode: 404);

        var costPerUnit = TroopsConfig.Get(order.Type).TrainingCost;
        var uncompleted = order.Amount - order.Completed;
        village.Resources = village.Resources.Add(costPerUnit.Multiply(uncompleted));

        if (order.JobId is not null)
            scheduler.DeleteJob(order.JobId);

        village.TrainOrders.Remove(order);

        // Recalculate timing for remaining orders — queue shifts up
        var remaining = village.TrainOrders.OrderBy(o => o.StartedAt).ToList();
        for (var i = 1; i < remaining.Count; i++)
        {
            var duration = remaining[i].CompletesAt - remaining[i].StartedAt;
            remaining[i].StartedAt = remaining[i - 1].CompletesAt;
            remaining[i].CompletesAt = remaining[i].StartedAt + duration;
        }

        await repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}
