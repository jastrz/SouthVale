using Hangfire;
using TownManager.Application.Interfaces;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs;

public class TroopMovementResolutionJob(
    IMovementRepository repo,
    IUnitOfWork uow,
    IEnumerable<IMovementResolver> resolvers)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ResolveAsync(Guid movementId, CancellationToken ct)
    {
        var movement = await repo.GetByIdAsync(movementId, ct);
        if (movement is null) return;
 
        // Recall command may have fired first
        if (movement.Status != MovementStatus.InFlight) return;
 
        // Already resolved 
        if (movement.CompletedAt.HasValue) return;
 
        var resolver = resolvers.FirstOrDefault(r => r.Handles == movement.Type)
                       ?? throw new InvalidOperationException(
                           $"No resolver registered for MovementType.{movement.Type}");
 
        await resolver.ResolveAsync(movement, ct);
 
        movement.CompletedAt = DateTime.UtcNow;
        movement.Status = MovementStatus.Resolved;
 
        await uow.SaveChangesAsync(ct);
    }
}
