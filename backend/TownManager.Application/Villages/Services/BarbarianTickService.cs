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
    IPlayerRepository playerRepo,
    IMapService mapService,
    IMediator mediator) : IBarbarianTickService
{
    private int? _maxPlayerTroops;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var cap = await GetPlayerTroopCap(ct);
        var barbarians = await villageRepo.GetBarbarianVillagesAsync(BarbarianConfig.BarbarianPlayerId, ct);
        var rng = Random.Shared;

        // per-village troop cap - each barb village stays below best player's total troops
        foreach (var b in barbarians)
        {
            if (rng.Next(2) == 0)
            {
                await AutoBuild(b, ct);
                await AutoTrain(b, cap, ct);
            }
            else
            {
                await AutoTrain(b, cap, ct);
                await AutoBuild(b, ct);
            }
            await TryAttack(b, rng, ct);
        }

        await Replenish(barbarians.Count, rng, ct);

        await villageRepo.SaveChangesAsync(ct);
    }

    private async Task<int> GetPlayerTroopCap(CancellationToken ct)
    {
        if (_maxPlayerTroops.HasValue) return _maxPlayerTroops.Value;
        var players = await playerRepo.GetAllPlayersWithTroopDataAsync(ct);
        var maxVillage = players
            .Where(p => p.Id != BarbarianConfig.BarbarianPlayerId)
            .SelectMany(p => p.Villages)
            .Select(v =>
                v.Troops.TotalCount +
                v.TroopMovements
                    .Where(m => m.Status == MovementStatus.InFlight)
                    .Sum(m => m.Troops.TotalCount))
            .DefaultIfEmpty(0)
            .Max();
        _maxPlayerTroops = (int)(maxVillage * BarbarianConfig.MaxTroopRatio);
        return _maxPlayerTroops.Value;
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

    private async Task AutoTrain(Village b, int cap, CancellationToken ct)
    {
        var totalAll = b.Troops.TotalCount +
            b.TrainOrders.Sum(o => o.Amount - o.Completed) +
            b.TroopMovements.Where(m => m.Status == MovementStatus.InFlight).Sum(m => m.Troops.TotalCount);
        var remainingGlobal = cap - totalAll;
        if (remainingGlobal <= 0) return;

        var effects = BuildingConfig.AggregateEffects(b.Buildings);
        b.Tick(effects);

        var available = b.Resources.Clone();
        var orders = new List<TroopEntry>();

        foreach (var type in new[] { TroopType.Swordsman, TroopType.Archer, TroopType.Dogs, TroopType.Horsemen, TroopType.LlamaRiders })
        {
            var current = b.Troops.Get(type);

            var inTraining = b.TrainOrders
                .Where(o => o.Type == type)
                .Sum(o => o.Amount - o.Completed);

            var inFlight = b.TroopMovements
                .Where(m => m.Status == MovementStatus.InFlight)
                .Sum(m => m.Troops.Get(type));
                
            var perTypeCap = BarbarianConfig.MaxTroops.Get(type) - (current + inTraining + inFlight);
            var deficit = Math.Min(perTypeCap, remainingGlobal);
            if (deficit <= 0) continue;

            var config = TroopsConfig.Get(type);
            var hasBuilding = b.Buildings.Any(b => b.Type == config.TrainedAt && b.Level >= 1);
            if (!hasBuilding) continue;

            var cost = config.TrainingCost;
            var maxAffordable = int.MaxValue;
            if (cost.Wood > 0) maxAffordable = Math.Min(maxAffordable, (int)(available.Wood / cost.Wood));
            if (cost.Clay > 0) maxAffordable = Math.Min(maxAffordable, (int)(available.Clay / cost.Clay));
            if (cost.Iron > 0) maxAffordable = Math.Min(maxAffordable, (int)(available.Iron / cost.Iron));
            if (cost.Beer > 0) maxAffordable = Math.Min(maxAffordable, (int)(available.Beer / cost.Beer));
            if (maxAffordable == int.MaxValue) maxAffordable = deficit;

            var toTrain = Math.Min(deficit, maxAffordable);
            if (toTrain <= 0) continue;

            available = available.Subtract(cost.Multiply(toTrain));
            orders.Add(new TroopEntry(type, toTrain));
            remainingGlobal -= toTrain;
        }

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
        foreach (var type in new[] { TroopType.Swordsman, TroopType.Archer, TroopType.Dogs, TroopType.Horsemen, TroopType.LlamaRiders })
        {
            var count = b.Troops.Get(type);
            if (count > 0) troops.Add(new TroopEntry(type, (int)Math.Ceiling(count / 2.0)));
        }

        await mediator.Send(new CreateAttackOrderCommand(b.Id, troops, target.Id), ct);
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
                Troops = BarbarianConfig.StartingTroops.Clone(),
                Resources = new Resources(rc.Wood, rc.Clay, rc.Iron, rc.Beer),
                Coordinates = coords,
                Buildings = BarbarianConfig.StartingBuildings
                    .Select(kv => Building.Create(kv.Key, kv.Value))
                    .ToList()
            });

            Log?.Invoke(BarbarianConfig.BarbarianPlayerId.ToString(), coords.ToString()!, "barbarian-spawn", null);
        }
    }
}
