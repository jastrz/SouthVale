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
        var home = await villageRepo.GetForCombatAsync(movement.VillageId, ct);
        if (home is null)
        {
            logger.LogWarning("Return target village {VillageId} not found for movement {MovementId}", movement.VillageId, movement.Id);
            return;
        }

        home.Troops = home.Troops.Add(movement.Troops);

        var loot = movement.CarriedResources;
        if (loot is not null)
            home.Resources = home.Resources.Add(loot);

        var fromName = home.Name;
        var fromPlayer = home.Player?.Username ?? "";
        if (movement.TargetVillageId.HasValue && movement.TargetVillageId != movement.VillageId)
        {
            var from = await villageRepo.GetForCombatAsync(movement.TargetVillageId.Value, ct);
            if (from is not null)
            {
                fromName = from.Name;
                fromPlayer = from.Player?.Username ?? "";
            }
        }

        await reportRepo.AddAsync(
            ReportFactory.ReturnReport(home.PlayerId, home.Name, fromName, fromPlayer, movement.Troops, loot), ct);

        await db.SaveChangesAsync(ct);

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(home.PlayerId, ct);
        if (userId is not null)
        {
            await notifications.VillageUpdatedAsync(userId, home.Id, ct);
            await notifications.ReportCreatedAsync(userId, ct);
        }

        logger.LogInformation(
            "Return movement {MovementId} resolved: {TroopsSummary} returned to village {VillageId}",
            movement.Id, movement.Troops.TotalCount, movement.VillageId);
    }
}
