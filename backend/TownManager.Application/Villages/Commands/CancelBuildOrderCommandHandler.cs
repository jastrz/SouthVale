using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Commands;

public class CancelBuildOrderCommandHandler(IVillageRepository repo, IJobScheduler scheduler)
    : IRequestHandler<CancelBuildOrderCommand, Result>
{
    public async Task<Result> Handle(CancelBuildOrderCommand request, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAndOrdersAsync(request.OrderId, ct);
        if (village is null)
            return Result.Failure(["Build order not found."], statusCode: 404);

        var order = village.BuildOrders.FirstOrDefault(o => o.Id == request.OrderId);
        if (order is null)
            return Result.Failure(["Build order not found."], statusCode: 404);

        var isLatest = village.BuildOrders
            .Where(o => o.BuildingType == order.BuildingType)
            .All(o => o.TargetLevel <= order.TargetLevel);
        if (!isLatest)
            return Result.Failure(["Can only cancel the latest upgrade order for this building."]);

        var cost = BuildingConfig.Get(order.BuildingType, order.TargetLevel).UpgradeCost;
        village.Resources = village.Resources.Add(cost);

        if (order.JobId is not null)
            scheduler.DeleteJob(order.JobId);

        village.BuildOrders.Remove(order);

        // Recalculate timing for remaining orders — queue shifts up
        var remaining = village.BuildOrders.OrderBy(o => o.StartsAt).ToList();
        for (var i = 1; i < remaining.Count; i++)
        {
            var duration = remaining[i].CompletesAt - remaining[i].StartsAt;
            remaining[i].StartsAt = remaining[i - 1].CompletesAt;
            remaining[i].CompletesAt = remaining[i].StartsAt + duration;
        }

        await repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}
