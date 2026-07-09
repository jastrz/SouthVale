using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Factories;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

public class SettleMovementResolver(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    IReportRepository reportRepo,
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

            TimeSpan travelTime = movement.ArrivesAt - movement.DepartureAt;

            var returnMovement = TroopMovement.Create(
                returningSettlers,
                movement.VillageId,
                travelTime,
                DateTime.UtcNow,
                MovementType.Return);

            origin.TroopMovements.Add(returnMovement);

            await reportRepo.AddAsync(
                ReportFactory.SettleFailedReport(origin.PlayerId, movement.TargetCoordinates.X, movement.TargetCoordinates.Y), ct);

            await db.SaveChangesAsync(ct);

            var userId = await playerRepo.GetUserIdByPlayerIdAsync(origin.PlayerId, ct);
            if (userId is not null)
            {
                await notifications.VillageUpdatedAsync(userId, origin.Id, ct);
                await notifications.ReportCreatedAsync(userId, ct);
            }

            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);

            VillageActivity.Log?.Invoke(origin.PlayerId.ToString(), origin.Name, "settle-fail",
                new { movement.TargetCoordinates });

            return;
        }

        var newVillage = Village.CreateStarter(
            $"{origin.Player?.Username ?? "Player"}'s village ({movement.TargetCoordinates.X}|{movement.TargetCoordinates.Y})",
            movement.TargetCoordinates);

        newVillage.PlayerId = origin.PlayerId;

        villageRepo.Add(newVillage);

        await reportRepo.AddAsync(
            ReportFactory.SettleReport(origin.PlayerId, newVillage.Name, movement.TargetCoordinates.X, movement.TargetCoordinates.Y), ct);

        await db.SaveChangesAsync(ct);

        var userId2 = await playerRepo.GetUserIdByPlayerIdAsync(origin.PlayerId, ct);
        if (userId2 is not null)
        {
            await notifications.VillagesChangedAsync(userId2, ct);
            await notifications.ReportCreatedAsync(userId2, ct);
        }

        VillageActivity.Log?.Invoke(origin.PlayerId.ToString(), origin.Name, "settle-success",
            new { NewVillage = newVillage.Name, movement.TargetCoordinates });

        logger.LogInformation("New village {VillageName} created at {Coords} by player {PlayerId}",
            newVillage.Name, movement.TargetCoordinates, origin.PlayerId);
    }
}
