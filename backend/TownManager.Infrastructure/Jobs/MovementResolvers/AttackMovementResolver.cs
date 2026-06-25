using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Persistence;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Services;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles attack arrival: combat resolution, loot calculation, survivor return.
/// </summary>
public class AttackMovementResolver(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    AppDbContext db,
    IJobScheduler scheduler,
    IGameNotificationService notifications,
    ILogger<AttackMovementResolver> logger
    ) : IMovementResolver
{
    public MovementType Handles => MovementType.Attack;

    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        var village = await villageRepo.GetWithMovementOrdersAsync(movement.VillageId, ct);
        if (village is null)
        {
            logger.LogWarning("Source village {VillageId} not found for attack movement {MovementId}", movement.VillageId, movement.Id);
            return;
        }

        var targetVillage = await villageRepo.GetForCombatAsync(movement.TargetVillageId!.Value, ct);
        if (targetVillage is null)
        {
            logger.LogWarning("Target village {TargetVillageId} not found for attack movement {MovementId}",
                movement.TargetVillageId, movement.Id);
            return;
        }

        var combatResult = CombatResolver.Resolve(movement.Troops, targetVillage.Troops, targetVillage.Resources);

        targetVillage.Troops = combatResult.DefenderTroops;

        if (!combatResult.AttackerTroops.IsEmpty())
        {
            // var travelTime = movement.ArrivesAt - movement.DepartureAt;
            var travelTime = TimeSpan.FromSeconds(10);

            var returnMovement = TroopMovement.Create(combatResult.AttackerTroops, combatResult.AttackerLoot, movement.VillageId,
                travelTime, DateTime.UtcNow, MovementType.Return);

            village.TroopMovements.Add(returnMovement);

            await db.SaveChangesAsync(ct);

            var sourceUserId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
            if (sourceUserId is not null)
                await notifications.VillageUpdatedAsync(sourceUserId, village.Id, ct);

            if (targetVillage.PlayerId != village.PlayerId)
            {
                var targetUserId = await playerRepo.GetUserIdByPlayerIdAsync(targetVillage.PlayerId, ct);
                if (targetUserId is not null)
                    await notifications.VillageUpdatedAsync(targetUserId, targetVillage.Id, ct);
            }

            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);
        }
    }
}
