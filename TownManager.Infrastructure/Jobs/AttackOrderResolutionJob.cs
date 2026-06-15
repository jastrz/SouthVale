using Hangfire;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

public class AttackOrderResolutionJob(IVillageRepository repo, IUnitOfWork uow)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, CancellationToken ct)
    {
        var village = await repo.GetWithAttackOrdersAsyncByOrder(orderId, ct);
        if (village is null) return;

        var order = village.TroopMovements.FirstOrDefault(o => o.Id == orderId);
        if (order is null) return;

        if (order.TargetVillageId.HasValue)
        {
            var targetVillage = await repo.GetForCombatAsync(order.TargetVillageId.Value, ct);
            if (targetVillage is null) return;
        }
        else
        {
            return;
        }

        // Execute attack logic (calculate damage, loot, etc.)
        order.CompletedAt = DateTime.UtcNow;

        // ... attack resolution logic ...
        village.Troops = village.Troops.Add(order.Troops);

        village.TroopMovements.Remove(order);
        
        await uow.SaveChangesAsync(ct);
    }
}
