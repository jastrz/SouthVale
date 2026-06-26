using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Factories;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

public class ReturnMovementResolver(
    IVillageRepository villageRepo,
    IPlayerRepository playerRepo,
    IReportRepository reportRepo,
    AppDbContext db,
    IGameNotificationService notifications,
    ILogger<ReturnMovementResolver> logger) : IMovementResolver
{
    public MovementType Handles => MovementType.Return;

    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        var village = await villageRepo.GetForCombatAsync(movement.VillageId, ct);
        if (village is null)
        {
            logger.LogWarning("Return target village {VillageId} not found for movement {MovementId}", movement.VillageId, movement.Id);
            return;
        }

        village.Troops = village.Troops.Add(movement.Troops);

        var loot = movement.CarriedResources;
        if (loot is not null)
            village.Resources = village.Resources.Add(loot);

        await reportRepo.AddAsync(
            ReportFactory.ReturnReport(village.PlayerId, village.Name, movement.Troops, loot), ct);

        await db.SaveChangesAsync(ct);

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
        if (userId is not null)
        {
            await notifications.VillageUpdatedAsync(userId, village.Id, ct);
            await notifications.ReportCreatedAsync(userId, ct);
        }

        logger.LogInformation(
            "Return movement {MovementId} resolved: {TroopsSummary} returned to village {VillageId}",
            movement.Id, movement.Troops.TotalCount, movement.VillageId);
    }
}
