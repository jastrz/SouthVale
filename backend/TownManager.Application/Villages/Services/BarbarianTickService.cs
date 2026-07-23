using MediatR;
using TownManager.Application.Interfaces;
using TownManager.Application.Map.Services;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using static TownManager.Application.Villages.VillageActivity;

namespace TownManager.Application.Villages.Services;

public interface IBarbarianTickService
{
    Task ExecuteAsync(CancellationToken ct);
}

public class BarbarianTickService(
    IVillageRepository villageRepo,
    IMapService mapService,
    IMediator mediator) : IBarbarianTickService
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var barbarians = await villageRepo.GetBarbarianVillagesAsync(BarbarianConfig.BarbarianPlayerId, ct);
        var rng = Random.Shared;

        foreach (var b in barbarians)
        {
            await AutoBuild(b, ct);
            await AutoTrain(b, ct);
            await TryAttack(b, rng, ct);
        }

        await Replenish(barbarians.Count, rng, ct);
    }

    private async Task AutoBuild(Village b, CancellationToken ct)
    {
        foreach (var buildingType in BarbarianConfig.StartingBuildings.Keys)
        {
            var building = b.Buildings.FirstOrDefault(x => x.Type == buildingType);
            var currentLevel = building?.Level ?? 0;
            if (currentLevel >= BarbarianConfig.MaxBuildingLevel) continue;

            var hasQueuedOrder = b.BuildOrders.Any(o => o.BuildingType == buildingType);
            if (hasQueuedOrder) continue;

            var nextLevel = currentLevel + 1;
            var lastQueued = await villageRepo.GetMaxBuildOrderTargetAsync(b.Id, buildingType, ct);
            if ((lastQueued ?? 0) >= nextLevel) continue;

            await mediator.Send(new CreateBuildOrderCommand(b.Id, buildingType), ct);
        }
    }

    private async Task AutoTrain(Village b, CancellationToken ct)
    {
        var orders = new List<TroopEntry>();
        void Check(TroopType t, int current, int max)
        {
            var deficit = max - current;
            if (deficit > 0) orders.Add(new TroopEntry(t, deficit));
        }

        Check(TroopType.Swordsman, b.Troops.Get(TroopType.Swordsman), BarbarianConfig.MaxTroops.Get(TroopType.Swordsman));
        Check(TroopType.Archer, b.Troops.Get(TroopType.Archer), BarbarianConfig.MaxTroops.Get(TroopType.Archer));

        if (orders.Count > 0)
            await mediator.Send(new CreateTrainOrderCommand(b.Id, orders), ct);
    }

    private async Task TryAttack(Village b, Random rng, CancellationToken ct)
    {
        if (b.Troops.IsEmpty()) return;
        if (b.LastAttackAt.HasValue &&
            DateTime.UtcNow - b.LastAttackAt.Value < BarbarianConfig.AttackCooldown)
            return;

        var hasOutgoingAttack = b.TroopMovements.Any(m =>
            m.Type == MovementType.Attack && m.Status == MovementStatus.InFlight);
        if (hasOutgoingAttack) return;

        var nearby = await villageRepo.GetForMapWithinRadius(b.Coordinates, BarbarianConfig.AttackRange, ct);
        var targets = nearby.Where(v => v.PlayerId != BarbarianConfig.BarbarianPlayerId && v.Id != b.Id && v.Troops.TotalCount > b.Troops.TotalCount).ToList();
        if (targets.Count == 0) return;

        var target = targets[rng.Next(targets.Count)];

        var troops = new List<TroopEntry>();
        if (b.Troops.Get(TroopType.Swordsman) > 0) troops.Add(new TroopEntry(TroopType.Swordsman, (int)Math.Ceiling(b.Troops.Get(TroopType.Swordsman) / 2.0)));
        if (b.Troops.Get(TroopType.Archer) > 0) troops.Add(new TroopEntry(TroopType.Archer, (int)Math.Ceiling(b.Troops.Get(TroopType.Archer) / 2.0)));

        await mediator.Send(new CreateAttackOrderCommand(b.Id, troops, target.Id), ct);
        b.LastAttackAt = DateTime.UtcNow;
    }

    private async Task Replenish(int currentCount, Random rng, CancellationToken ct)
    {
        var deficit = BarbarianConfig.TargetPopulation - currentCount;
        if (deficit <= 0) return;

        var free = await mapService.GetFreeTilesAsync(deficit, ct);

        foreach (var coords in free)
        {
            var rc = BarbarianConfig.StartingResources;
            villageRepo.Add(new Village
            {
                Name = $"Barbarian ({coords.X}|{coords.Y})",
                VillageType = VillageType.Barbarian,
                PlayerId = BarbarianConfig.BarbarianPlayerId,
                Troops = new Troops(BarbarianConfig.StartingTroops.Get(TroopType.Swordsman), BarbarianConfig.StartingTroops.Get(TroopType.Archer)),
                Resources = new Resources(rc.Wood, rc.Clay, rc.Iron, rc.Beer),
                Coordinates = coords,
                Buildings = BarbarianConfig.StartingBuildings
                    .Select(kv => Building.Create(kv.Key, kv.Value))
                    .ToList()
            });

            Log?.Invoke(BarbarianConfig.BarbarianPlayerId.ToString(), coords.ToString()!, "barbarian-spawn", null);
        }

        await villageRepo.SaveChangesAsync(ct);
    }
}
