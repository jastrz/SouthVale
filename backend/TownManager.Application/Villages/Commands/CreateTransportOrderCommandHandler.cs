using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Services;

namespace TownManager.Application.Villages.Commands;

public record CreateTransportOrderCommand(
    Guid VillageId,
    Guid TargetVillageId,
    IReadOnlyList<TroopEntry> Troops,
    ResourcesDto Resources
) : IRequest<Result>;

public class CreateTransportOrderCommandHandler(
    IVillageRepository repo,
    IPlayerRepository playerRepo,
    IGameNotificationService notifications,
    IJobScheduler scheduler
) : IRequestHandler<CreateTransportOrderCommand, Result>
{
    public async Task<Result> Handle(CreateTransportOrderCommand request, CancellationToken ct)
    {
        var village = await repo.GetWithMovementOrdersAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Village not found."], statusCode: 404);

        var targetVillage = await repo.GetByIdAsync(request.TargetVillageId, ct);
        if (targetVillage is null)
            return Result.Failure(["Target village not found."], statusCode: 404);

        if (targetVillage.PlayerId != village.PlayerId)
            return Result.Failure(["Can only transport to your own villages."], statusCode: 403);

        var troops = new Troops(
            request.Troops.Sum(t => t.TroopType == TroopType.Swordsman ? t.Count : 0),
            request.Troops.Sum(t => t.TroopType == TroopType.Archer ? t.Count : 0),
            request.Troops.Sum(t => t.TroopType == TroopType.Settler ? t.Count : 0)
        );
        
        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);

        if (!village.Troops.HasEnough(troops))
            return Result.Failure(["Not enough troops in garrison."]);

        var sentResources = new Resources(request.Resources.Wood, request.Resources.Clay, request.Resources.Iron, request.Resources.Crop);
        if (!village.Resources.CanAfford(sentResources))
            return Result.Failure(["Not enough resources."]);

        var totalCarry = request.Troops
            .Sum(t => TroopsConfig.All[t.TroopType].Stats.CarryCapacity * t.Count);
        var totalSent = sentResources.Wood + sentResources.Clay + sentResources.Iron + sentResources.Crop;
        if (totalSent > totalCarry)
            return Result.Failure(["Resources exceed troop carry capacity."]);

        village.Troops = village.Troops.Subtract(troops);
        village.Resources = village.Resources.Subtract(sentResources);

        var slowestSpeed = request.Troops
            .Where(t => t.Count > 0)
            .Min(t => TroopsConfig.All[t.TroopType].Stats.Speed);
        var travelTime = TravelTimeCalculator.Calculate(village.Coordinates, targetVillage.Coordinates, slowestSpeed);

        var movement = TroopMovement.Create(troops, sentResources, request.TargetVillageId, travelTime,
            DateTime.UtcNow, MovementType.Transport);

        village.TroopMovements.Add(movement);
        await repo.SaveChangesAsync(ct);

        var targetUserId = await playerRepo.GetUserIdByPlayerIdAsync(targetVillage.PlayerId, ct);
        if (targetUserId is not null)
            await notifications.MovementsChangedAsync(targetUserId, ct);

        scheduler.ScheduleMovementResolution(movement.Id, travelTime);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "transport",
            new { Target = targetVillage.Name, TargetId = request.TargetVillageId, Troops = troops, Resources = sentResources });

        return Result.Success();
    }
}
