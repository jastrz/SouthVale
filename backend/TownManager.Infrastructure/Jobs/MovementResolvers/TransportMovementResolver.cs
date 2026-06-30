using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Factories;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

public class TransportMovementResolver(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    IReportRepository reportRepo,
    AppDbContext db,
    IGameNotificationService notifications,
    ILogger<TransportMovementResolver> logger) : IMovementResolver
{
    public MovementType Handles => MovementType.Transport;

    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        var targetVillage = await villageRepo.GetForCombatAsync(movement.TargetVillageId!.Value, ct);
        if (targetVillage is null)
        {
            logger.LogWarning("Target village {TargetVillageId} not found for transport movement {MovementId}",
                movement.TargetVillageId, movement.Id);
            return;
        }

        var originVillage = await villageRepo.GetByIdAsync(movement.VillageId, ct);
        if (originVillage is null)
        {
            logger.LogWarning("Origin village {VillageId} not found for transport movement {MovementId}",
                movement.VillageId, movement.Id);
            return;
        }

        targetVillage.Troops = targetVillage.Troops.Add(movement.Troops);
        if (movement.CarriedResources is not null)
            targetVillage.Resources = targetVillage.Resources.Add(movement.CarriedResources);

        await reportRepo.AddAsync(
            ReportFactory.TransportReport(targetVillage.PlayerId, originVillage.Name, targetVillage.Name,
                movement.Troops, movement.CarriedResources ?? Resources.Zero), ct);

        await db.SaveChangesAsync(ct);

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(targetVillage.PlayerId, ct);
        if (userId is not null)
        {
            await notifications.VillageUpdatedAsync(userId, targetVillage.Id, ct);
            await notifications.ReportCreatedAsync(userId, ct);
        }
    }
}
