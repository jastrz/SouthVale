using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Services;

namespace TownManager.Application.Villages.Commands;

public class CreateAttackOrderCommandHandler(
    IVillageRepository repo,
    IPlayerRepository playerRepo,
    IGameNotificationService notifications,
    IJobScheduler scheduler
    )
    : IRequestHandler<CreateAttackOrderCommand, Result>
{
    public async Task<Result> Handle(CreateAttackOrderCommand request, CancellationToken ct)
    {
        var troops = new Troops();
        foreach (var t in request.Troops.Where(t => t.Count > 0))
            troops = troops.Add(t.TroopType, t.Count);

        var village = await repo.GetWithMovementOrdersAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Player village not found."], statusCode: 404);

        var targetVillage = await repo.GetByIdAsync(request.TargetVillageId, ct);
        if (targetVillage is null)
            return Result.Failure(["Target village not found."], statusCode: 404);

        // Update Village state
        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.Tick(effects);

        // Validate troops are available in garrison
        if (!village.Troops.HasEnough(troops))
            return Result.Failure(["Not enough troops in garrison."]);

        // Remove troops from garrison
        village.Troops = village.Troops.Subtract(troops);

        var slowestSpeed = request.Troops
            .Where(t => t.Count > 0)
            .Min(t => TroopsConfig.All[t.TroopType].Stats.Speed);
        var travelTime = TravelTimeCalculator.Calculate(village.Coordinates, targetVillage.Coordinates, slowestSpeed);
        
        var order = TroopMovement.Create(troops, request.TargetVillageId, travelTime, 
            DateTime.UtcNow, MovementType.Attack);
        
        village.TroopMovements.Add(order);

        await repo.SaveChangesAsync(ct);

        // Notify target player of incoming movement
        if (targetVillage.PlayerId != village.PlayerId)
        {
            var targetUserId = await playerRepo.GetUserIdByPlayerIdAsync(targetVillage.PlayerId, ct);
            if (targetUserId is not null)
                await notifications.MovementsChangedAsync(targetUserId, ct);
        }

        // Schedule resolution when attack arrives
        scheduler.ScheduleMovementResolution(order.Id, travelTime);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "attack",
            new { Target = targetVillage.Name, TargetId = request.TargetVillageId, Troops = troops });

        return Result.Success();
    }
}
