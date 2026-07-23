using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Factories;
using TownManager.Infrastructure.Persistence;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Services;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

public class AttackMovementResolver(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    IReportRepository reportRepo,
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
            logger.LogWarning("Target village {TargetVillageId} not found for attack movement {MovementId} — returning troops",
                movement.TargetVillageId, movement.Id);

            var home = await villageRepo.GetWithMovementOrdersAsync(movement.VillageId, ct);
            if (home is not null)
            {
                var travelTime = movement.ArrivesAt - movement.DepartureAt;
                var returnMovement = TroopMovement.Create(
                    movement.Troops,
                    Resources.Zero, movement.VillageId,
                    travelTime, DateTime.UtcNow, MovementType.Return);
                home.TroopMovements.Add(returnMovement);

                await reportRepo.AddAsync(
                    ReportFactory.AttackCancelledReport(village.PlayerId, village.Name, movement.TargetVillageId.ToString()!), ct);
                
                await db.SaveChangesAsync(ct);

                scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);

                var userId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
                if (userId is not null)
                    await notifications.ReportCreatedAsync(userId, ct);
            }
            return;
        }

        // Applying effects
        var defenderEffects = BuildingConfig.AggregateEffects(targetVillage.Buildings);
        targetVillage.Tick(defenderEffects);

        var playerVillages = await villageRepo.GetFullDetailsByPlayerAsync(village.PlayerId, ct);
        var allBuildings = playerVillages.SelectMany(v => v.Buildings);
        var attackerEffects = BuildingConfig.AggregateEffects(allBuildings);

        var originalDefenders = targetVillage.Troops;
        var crannyCap = defenderEffects.CrannyCapacity;
        var lootable = crannyCap > 0
            ? new Resources(
                Math.Max(0, targetVillage.Resources.Wood - crannyCap),
                Math.Max(0, targetVillage.Resources.Clay - crannyCap),
                Math.Max(0, targetVillage.Resources.Iron - crannyCap),
                Math.Max(0, targetVillage.Resources.Beer - crannyCap))
            : targetVillage.Resources;
        
        
        // Combat resolution
        var combatResult = CombatResolver.Resolve(movement.Troops, originalDefenders, lootable,
            defenderEffects.DefenseMultiplier,
            attackerEffects.BarracksAttackMultiplier,
            attackerEffects.StableAttackMultiplier);

        targetVillage.Troops = combatResult.DefenderTroops;
        targetVillage.Resources = targetVillage.Resources.Subtract(combatResult.AttackerLoot);

        var isBarbarianTarget = targetVillage.VillageType == VillageType.Barbarian;
        var destroyBarbarian = isBarbarianTarget && combatResult.DefenderTroops.IsEmpty();

        await reportRepo.AddAsync(
            ReportFactory.AttackReport(village.PlayerId, village.Name, village.Player.Username,
                targetVillage.Name, targetVillage.Player.Username,
                movement.Troops, combatResult.AttackerTroops, originalDefenders, combatResult.DefenderTroops, combatResult.AttackerLoot), ct);

        if (!isBarbarianTarget && targetVillage.PlayerId != village.PlayerId)
            await reportRepo.AddAsync(
                ReportFactory.DefenseReport(targetVillage.PlayerId, targetVillage.Name,
                    village.Name, village.Player.Username,
                    movement.Troops, combatResult.AttackerTroops, originalDefenders, combatResult.DefenderTroops, combatResult.AttackerLoot), ct);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "attack-resolved",
            new { Target = targetVillage.Name, TargetId = targetVillage.Id, AttackerWon = !combatResult.AttackerTroops.IsEmpty(), TargetDestroyed = destroyBarbarian });

        if (!combatResult.AttackerTroops.IsEmpty())
        {
            var slowestSpeed = TroopsConfig.GetSlowestSpeed(combatResult.AttackerTroops);
            var travelTime = TravelTimeCalculator.Calculate(targetVillage.Coordinates, village.Coordinates, slowestSpeed);

            var returnMovement = TroopMovement.Create(combatResult.AttackerTroops, combatResult.AttackerLoot, movement.TargetVillageId!.Value,
                travelTime, DateTime.UtcNow, MovementType.Return);

            village.TroopMovements.Add(returnMovement);

            if (destroyBarbarian) villageRepo.Remove(targetVillage);
            await db.SaveChangesAsync(ct);

            var sourceUserId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
            if (sourceUserId is not null)
            {
                await notifications.VillageUpdatedAsync(sourceUserId, village.Id, ct);
                await notifications.ReportCreatedAsync(sourceUserId, ct);
                if (destroyBarbarian)
                    await notifications.VillageDestroyedAsync(sourceUserId, ct);
            }

            if (!isBarbarianTarget && targetVillage.PlayerId != village.PlayerId)
            {
                var targetUserId = await playerRepo.GetUserIdByPlayerIdAsync(targetVillage.PlayerId, ct);
                if (targetUserId is not null)
                {
                    await notifications.VillageUpdatedAsync(targetUserId, targetVillage.Id, ct);
                    await notifications.ReportCreatedAsync(targetUserId, ct);
                }
            }

            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);
        }
        else
        {
            if (destroyBarbarian) villageRepo.Remove(targetVillage);
            await db.SaveChangesAsync(ct);

            var sourceUserId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
            if (sourceUserId is not null)
            {
                await notifications.ReportCreatedAsync(sourceUserId, ct);
                if (destroyBarbarian)
                    await notifications.VillageDestroyedAsync(sourceUserId, ct);
            }

            if (!isBarbarianTarget && targetVillage.PlayerId != village.PlayerId)
            {
                var targetUserId = await playerRepo.GetUserIdByPlayerIdAsync(targetVillage.PlayerId, ct);
                if (targetUserId is not null)
                    await notifications.ReportCreatedAsync(targetUserId, ct);
            }
        }
    }
}
