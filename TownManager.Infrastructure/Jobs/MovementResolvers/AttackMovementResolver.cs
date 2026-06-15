using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles attack arrival: combat resolution, loot calculation, survivor return.
/// </summary>
public class AttackMovementResolver(
    IVillageRepository villageRepo,
    IUnitOfWork uow,
    IJobScheduler scheduler
    ) : IMovementResolver
{
    public MovementType Handles => MovementType.Attack;
 
    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        var village = await villageRepo.GetWithMovementOrdersAsync(movement.VillageId, ct);
        if (village is null) return;
        
        var targetVillage = await villageRepo.GetForCombatAsync(
            movement.TargetVillageId!.Value, ct);
        if (targetVillage is null) return;
 
        // TODO: proper combat model (ATK vs DEF, casualty formula, etc.)

        var survivingAttackers = new Troops(movement.Troops.Swordsmen, movement.Troops.Archers);
        var loot = new Resources(1000, 1000, 1000, 1000);
 
        if (!survivingAttackers.IsEmpty())
        {
            // var travelTime = movement.ArrivesAt - movement.DepartureAt;
            var travelTime = TimeSpan.FromSeconds(10);
            
            var returnMovement = TroopMovement.Create(survivingAttackers, loot, movement.VillageId, 
                travelTime, DateTime.UtcNow, MovementType.Return);
            
            village.TroopMovements.Add(returnMovement);
            
            await uow.SaveChangesAsync(ct);
            
            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);
        }
    }
}