using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public class CreateTrainOrderCommandHandler(IVillageRepository repo, IJobScheduler scheduler)
    : IRequestHandler<CreateTrainOrderCommand, Result>
{
    public async Task<Result> Handle(CreateTrainOrderCommand request, CancellationToken ct)
    {
        var village = await repo.GetWithActiveOrdersAsync(request.VillageId, ct);

        if (village is null)
            return Result.Failure(["Village not found."], statusCode: 404);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.Tick(effects);

        var villageCount = await repo.CountByPlayerAsync(village.PlayerId, ct);

        var existingSettlers = village.Troops.Get(TroopType.Settler)
            + village.TrainOrders.Where(o => o.Type == TroopType.Settler).Sum(o => o.Amount - o.Completed);

        var totalCost = Resources.Zero;

        foreach (var entry in request.Orders)
        {
            var config = TroopsConfig.Get(entry.TroopType);
            var hasBuilding = village.Buildings.Any(b => b.Type == config.TrainedAt && b.Level >= 1);
            if (!hasBuilding)
                return Result.Failure([$"{config.TrainedAt} required to train {entry.TroopType}"]);

            Resources cost;
            if (entry.TroopType == TroopType.Settler)
            {
                // geometric series: each settler ×2 more than previous
                var k = villageCount + existingSettlers - 1;
                var totalMultiplier = (int)Math.Round(Math.Pow(2, k) * (Math.Pow(2, entry.Count) - 1));
                cost = config.TrainingCost.Multiply(Math.Max(entry.Count, totalMultiplier));
            }
            else
            {
                cost = config.TrainingCost.Multiply(entry.Count);
            }
            totalCost = totalCost.Add(cost);
            if (entry.TroopType == TroopType.Settler)
                existingSettlers += entry.Count;
        }

        if (!village.Resources.CanAfford(totalCost))
            return Result.Failure(["Not enough resources."]);

        village.Resources = village.Resources.Subtract(totalCost);
        
        var queueStartTime = village.TrainOrders.Any()
            ? village.TrainOrders.Max(o => o.CompletesAt)  // append after last order
            : DateTime.UtcNow;                                      // queue is empty, start now

        var newOrders = new List<TrainOrder>();

        foreach (var entry in request.Orders)
        {
            var entryConfig = TroopsConfig.Get(entry.TroopType);
            var speedMult = entryConfig.TrainedAt == BuildingType.Stable
                ? effects.StableTrainingSpeed : effects.BarracksTrainingSpeed;
            var trainingTime = TroopsConfig.CalculateTrainingTime(
                entry.TroopType,
                entry.Count,
                speedMult);

            var timePerUnit = trainingTime / entry.Count;
            var order = TrainOrder.Create(entry.TroopType, entry.Count, timePerUnit, queueStartTime);
            village.TrainOrders.Add(order);
            newOrders.Add(order);
            queueStartTime = order.CompletesAt;
        }

        await repo.SaveChangesAsync(ct);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "train",
            new { Orders = request.Orders.Select(o => new { o.TroopType, o.Count }) });

        foreach (var order in newOrders)
        {
            var firstUnitDelay = order.StartsAt - DateTime.UtcNow + order.TimePerUnit;
            order.JobId = scheduler.ScheduleTrainOrderResolution(order.Id, firstUnitDelay);
        }
        
        await repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}