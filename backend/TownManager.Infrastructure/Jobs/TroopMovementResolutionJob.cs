using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Persistence;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Jobs;

public class TroopMovementResolutionJob(
    IMovementRepository repo,
    IPlayerRepository playerRepo,
    AppDbContext db,
    IEnumerable<IMovementResolver> resolvers,
    IGameNotificationService notifications,
    ILogger<TroopMovementResolutionJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public async Task ResolveAsync(Guid movementId, CancellationToken ct)
    {
        var movement = await repo.GetByIdAsync(movementId, ct);
        if (movement is null)
        {
            logger.LogWarning("Movement {MovementId} not found, skipping", movementId);
            return;
        }

        if (movement.Status != MovementStatus.InFlight)
        {
            logger.LogInformation("Movement {MovementId} already {Status}, skipping", movementId, movement.Status);
            return;
        }

        if (movement.CompletedAt.HasValue)
        {
            logger.LogInformation("Movement {MovementId} already completed, skipping", movementId);
            return;
        }

        var resolver = resolvers.FirstOrDefault(r => r.Handles == movement.Type);
        if (resolver is null)
        {
            logger.LogError("No resolver registered for movement type {MovementType} (movement {MovementId})", movement.Type, movementId);
            return;
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await resolver.ResolveAsync(movement, ct);

        movement.CompletedAt = DateTime.UtcNow;
        movement.Status = MovementStatus.Resolved;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var sourceUserId = await playerRepo.GetUserIdByVillageIdAsync(movement.VillageId, ct);
        if (sourceUserId is not null)
            await notifications.MovementsChangedAsync(sourceUserId, ct);

        if (movement.TargetVillageId.HasValue)
        {
            var targetUserId = await playerRepo.GetUserIdByVillageIdAsync(movement.TargetVillageId.Value, ct);
            if (targetUserId is not null && targetUserId != sourceUserId)
                await notifications.MovementsChangedAsync(targetUserId, ct);
        }
    }
}
