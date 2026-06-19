using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Persistence;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles attack arrival: combat resolution, loot calculation, survivor return.
/// </summary>
public class AttackMovementResolver(
    IVillageRepository villageRepo,
    AppDbContext db,
    IJobScheduler scheduler,
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

        var targetVillage = await villageRepo.GetForCombatAsync(
            movement.TargetVillageId!.Value, ct);
        if (targetVillage is null)
        {
            logger.LogWarning("Target village {TargetVillageId} not found for attack movement {MovementId}",
                movement.TargetVillageId, movement.Id);
            return;
        }

        // TODO: proper combat model (ATK vs DEF, casualty formula, etc.)

        var survivingAttackers = new Troops(movement.Troops.Swordsmen, movement.Troops.Archers, movement.Troops.Settlers);
        var loot = new Resources(1000, 1000, 1000, 1000);

        if (!survivingAttackers.IsEmpty())
        {
            // var travelTime = movement.ArrivesAt - movement.DepartureAt;
            var travelTime = TimeSpan.FromSeconds(10);

            var returnMovement = TroopMovement.Create(survivingAttackers, loot, movement.VillageId,
                travelTime, DateTime.UtcNow, MovementType.Return);

            village.TroopMovements.Add(returnMovement);

            await db.SaveChangesAsync(ct);

            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);
        }
    }
}
