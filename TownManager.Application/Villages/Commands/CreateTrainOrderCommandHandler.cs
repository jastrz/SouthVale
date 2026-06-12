using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Villages.Commands;

public class CreateTrainOrderCommandHandler(IVillageRepository repo, IUnitOfWork uow, IJobScheduler scheduler)
    : IRequestHandler<CreateTrainOrderCommand, Result>
{
    public async Task<Result> Handle(CreateTrainOrderCommand request, CancellationToken ct)
    {
        if (request.Orders.Count == 0)
            return Result.Failure(["No orders provided."]);

        if (request.Orders.Any(o => o.Count <= 0))
            return Result.Failure(["Count must be greater than 0."]);

        if (request.Orders.GroupBy(o => o.TroopType).Any(g => g.Count() > 1))
            return Result.Failure(["Duplicate troop types in order."]);

        var village = await repo.GetWithActiveOrdersAsync(request.VillageId, ct);

        if (village is null)
            return Result.Failure(["Village not found."]);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);

        var totalCost = Resources.Zero;

        foreach (var entry in request.Orders)
        {
            var config = TroopsConfig.Get(entry.TroopType);
            totalCost = totalCost.Add(config.TrainingCost.Multiply(entry.Count));
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
            var trainingTime = TroopsConfig.CalculateTrainingTime(
                entry.TroopType,
                entry.Count,
                effects.TrainingSpeedMultiplier);

            var timePerUnit = trainingTime / entry.Count;
            var order = TrainOrder.Create(entry.TroopType, entry.Count, timePerUnit, queueStartTime);
            village.TrainOrders.Add(order);
            newOrders.Add(order);
            queueStartTime = order.CompletesAt;
        }

        await uow.SaveChangesAsync(ct);

        foreach (var order in newOrders)
        {
            var firstUnitDelay = order.StartedAt - DateTime.UtcNow + order.TimePerUnit;
            scheduler.ScheduleTrainOrderResolution(order.Id, firstUnitDelay);
        }
        
        return Result.Success();
    }
}