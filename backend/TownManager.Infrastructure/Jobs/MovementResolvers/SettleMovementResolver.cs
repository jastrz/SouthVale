using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs.MovementResolvers;

/// <summary>
/// Handles settler arrival: found a new village at the target tile, owned by the same player.
/// If the tile is already occupied (race), the settlers travel back as a fresh Return movement.
/// </summary>
public class SettleMovementResolver(
    IVillageRepository villageRepo,
    IUnitOfWork uow,
    IJobScheduler scheduler) : IMovementResolver
{
    public MovementType Handles => MovementType.Settle;

    public async Task ResolveAsync(TroopMovement movement, CancellationToken ct)
    {
        if (movement.TargetMapX is null || movement.TargetMapY is null) return;

        var origin = await villageRepo.GetWithMovementOrdersAsync(movement.VillageId, ct);
        if (origin is null) return;

        if (await villageRepo.GetByCoordsAsync(movement.TargetMapX.Value, movement.TargetMapY.Value, ct) is not null)
        {
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

            await uow.SaveChangesAsync(ct);
            
            scheduler.ScheduleMovementResolution(returnMovement.Id, travelTime);
            
            return;
        }

        var newVillage = Village.CreateStarter(
            $"{origin.Name} Settlement",
            (movement.TargetMapX.Value, movement.TargetMapY.Value));
        
        newVillage.PlayerId = origin.PlayerId;

        villageRepo.Add(newVillage);
    }
}
