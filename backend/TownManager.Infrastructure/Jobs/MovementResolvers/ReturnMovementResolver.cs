using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles troop return: add troops (and any loot) back to origin village.
/// </summary>
public class ReturnMovementResolver(
    IVillageRepository villageRepo,
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

        if (movement.CarriedResources is not null)
            village.Resources = village.Resources.Add(movement.CarriedResources);

        logger.LogInformation(
            "Return movement {MovementId} resolved: {TroopsSummary} returned to village {VillageId}",
            movement.Id, movement.Troops.TotalCount, movement.VillageId);
    }
}
