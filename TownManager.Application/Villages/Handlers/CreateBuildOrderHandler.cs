using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Queries;
using TownManager.Domain.Config;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Villages.Handlers;

public class CreateBuildOrderHandler(IVillageRepository repo, IUnitOfWork uow, IJobScheduler scheduler)
    : IRequestHandler<CreateBuildOrderCommand, Result>
{
    public async Task<Result> Handle(CreateBuildOrderCommand cmd, CancellationToken ct)
    {
        var village = await repo.GetWithActiveOrdersAsync(cmd.VillageId, ct);
        if (village is null)
            return Result.Failure(["Village not found"]);

        var currentLevel = village.Buildings
            .FirstOrDefault(b => b.Type == cmd.BuildingType)?.Level ?? 0;

        var nextLevel = currentLevel + 1;
        var config = BuildingConfig.Get(cmd.BuildingType, nextLevel);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);

        if (!village.Resources.CanAfford(config.UpgradeCost))
            return Result.Failure(["Not enough resources"]);

        if (village.BuildOrders.Any())
            return Result.Failure(["Build queue is full"]);

        village.Resources = village.Resources.Subtract(config.UpgradeCost);

        var order = BuildOrder.Create(cmd.BuildingType, nextLevel, config.UpgradeTime);
        village.BuildOrders.Add(order);
        // uow.MarkAsAdded(order);

        await uow.SaveChangesAsync(ct);

        scheduler.ScheduleBuildOrderResolution(order.Id, config.UpgradeTime);

        return Result.Success();
    }
}