using TownManager.Application.Interfaces;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles troop return: add troops (and any loot) back to origin village.
/// </summary>
public class ReturnMovementResolver(
    IVillageRepository villageRepo, IUnitOfWork uow) : IMovementResolver
{
    public MovementType Handles => MovementType.Return;
 
    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        var village = await villageRepo.GetForCombatAsync(movement.VillageId, ct);
        if (village is null) return;
 
        village.Troops = village.Troops.Add(movement.Troops);
 
        if (movement.CarriedResources is not null)
            village.Resources = village.Resources.Add(movement.CarriedResources);
    }
}
