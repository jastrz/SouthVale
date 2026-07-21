using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Villages.Commands;

public class CreateBuildOrderHandler(IVillageRepository repo, IJobScheduler scheduler)
    : IRequestHandler<CreateBuildOrderCommand, Result>
{
    public async Task<Result> Handle(CreateBuildOrderCommand cmd, CancellationToken ct)
    {
        var village = await repo.GetWithActiveOrdersAsync(cmd.VillageId, ct);
        
        if (village is null)
            return Result.Failure(["Village not found"], statusCode: 404);

        var building = village.Buildings.FirstOrDefault(b => b.Type == cmd.BuildingType);

        var lastQueuedTarget = await repo.GetMaxBuildOrderTargetAsync(cmd.VillageId, cmd.BuildingType, ct);
        var nextLevel = (lastQueuedTarget ?? building?.Level ?? 0) + 1;
        var config = BuildingConfig.Get(cmd.BuildingType, nextLevel);
        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);

        if (!village.Resources.CanAfford(config.UpgradeCost))
            return Result.Failure(["Not enough resources"]);

        village.Resources = village.Resources.Subtract(config.UpgradeCost);

        var queueStartTime = village.BuildOrders.Any()
            ? village.BuildOrders.Max(o => o.CompletesAt)
            : DateTime.UtcNow;

        var adjustedTime = TimeSpan.FromTicks((long)(config.UpgradeTime.Ticks / effects.BuildSpeedMultiplier));

        var order = BuildOrder.Create(cmd.BuildingType, nextLevel, adjustedTime);
        order.StartsAt = queueStartTime;
        order.CompletesAt = queueStartTime.Add(adjustedTime);
        village.BuildOrders.Add(order);

        await repo.SaveChangesAsync(ct);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "build",
            new { cmd.BuildingType, Level = nextLevel });

        order.JobId = scheduler.ScheduleBuildOrderResolution(order.Id, adjustedTime);
        
        await repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}