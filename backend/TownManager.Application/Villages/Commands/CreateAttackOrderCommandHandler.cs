using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public class CreateAttackOrderCommandHandler(
    IVillageRepository repo, 
    IUnitOfWork uow, 
    IJobScheduler scheduler
    )
    : IRequestHandler<CreateAttackOrderCommand, Result>
{
    public async Task<Result> Handle(CreateAttackOrderCommand request, CancellationToken ct)
    {
        var troops = new Troops(
            request.Troops.Sum(t => t.TroopType == TroopType.Swordsman ? t.Count : 0),
            request.Troops.Sum(t => t.TroopType == TroopType.Archer ? t.Count : 0)
        );
        
        var village = await repo.GetWithMovementOrdersAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Player village not found."]);
        
        var targetVillage = await repo.GetByIdAsync(request.TargetVillageId, ct);
        if (targetVillage is null)
            return Result.Failure(["Target village not found."]);

        // Validate troops are available in garrison
        if (!village.Troops.HasEnough(troops))
            return Result.Failure(["Not enough troops in garrison."]);

        // Remove troops from garrison
        village.Troops = village.Troops.Subtract(troops);

        var travelTime = TimeSpan.FromSeconds(5); // Calculate based on distance
        
        var order = TroopMovement.Create(troops, request.TargetVillageId, travelTime, 
            DateTime.UtcNow, MovementType.Attack);
        
        village.TroopMovements.Add(order);

        await uow.SaveChangesAsync(ct);

        // Schedule resolution when attack arrives
        scheduler.ScheduleMovementResolution(order.Id, travelTime);

        return Result.Success();
    }
}
