using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Persistence;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles settler arrival: found a new village at the target tile, owned by the same player.
/// If the tile is already occupied (race), the settlers travel back as a fresh Return movement.
/// </summary>
public class SettleMovementResolver(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    AppDbContext db,
    IJobScheduler scheduler,
    IGameNotificationService notifications,
    ILogger<SettleMovementResolver> logger) : IMovementResolver
{
    public MovementType Handles => MovementType.Settle;

    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        if (movement.TargetCoordinates is null)
        {
            logger.LogWarning("Settle movement {MovementId} has no target coordinates, skipping", movement.Id);
            return;
        }

        var origin = await villageRepo.GetWithMovementOrdersAsync(movement.VillageId, ct);
        if (origin is null)
        {
            logger.LogWarning("Origin village {VillageId} not found for settle movement {MovementId}", movement.VillageId, movement.Id);
            return;
        }

        if (await villageRepo.GetByCoordsAsync(movement.TargetCoordinates, ct) is not null)
        {
            logger.LogInformation("Tile {Coords} already occupied, returning settlers to {VillageId}",
                movement.TargetCoordinates, movement.VillageId);

            var returningSettlers = new Troops(0, 0, movement.Troops.Settlers);
            if (returningSettlers.IsEmpty()) return;

            TimeSpan travelTime = movement.CompletedAt!.Value - movement.DepartureAt;

            var returnMovement = TroopMovement.Create(
                returningSettlers,
                movement.VillageId,
                travelTime,
                DateTime.UtcNow,
                MovementType.Return);

            origin.TroopMovements.Add(returnMovement);

            await db.SaveChangesAsync(ct);

            var userId = await playerRepo.GetUserIdByPlayerIdAsync(origin.PlayerId, ct);
            if (userId is not null)
                await notifications.VillageUpdatedAsync(userId, origin.Id, ct);

            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);

            return;
        }

        var newVillage = Village.CreateStarter(
            $"{origin.Name} Settlement",
            movement.TargetCoordinates);

        newVillage.PlayerId = origin.PlayerId;

        villageRepo.Add(newVillage);

        await db.SaveChangesAsync(ct);

        var userId2 = await playerRepo.GetUserIdByPlayerIdAsync(origin.PlayerId, ct);
        if (userId2 is not null)
            await notifications.VillagesChangedAsync(userId2, ct);

        logger.LogInformation("New village {VillageName} created at {Coords} by player {PlayerId}",
            newVillage.Name, movement.TargetCoordinates, origin.PlayerId);
    }
}
