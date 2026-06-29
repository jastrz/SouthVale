using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Persistence.Repositories;

public class MovementRepository(AppDbContext db) : IMovementRepository
{
    public Task<TroopMovement?> GetByIdAsync(Guid movementId, CancellationToken ct = default) =>
        db.TroopMovements.Include(t => t.Troops)
            .Include(t => t.CarriedResources)
            .FirstOrDefaultAsync(m => m.Id == movementId, ct);

    public async Task<IReadOnlyList<TroopMovement>> GetInFlightForPlayerAsync(Guid playerId, IReadOnlyList<Guid> playerVillageIds, CancellationToken ct = default) =>
        await db.TroopMovements
            .AsNoTracking()
            .Include(t => t.Troops)
            .Include(t => t.Village)
            .Where(t => t.Status == MovementStatus.InFlight && (
                t.Village.PlayerId == playerId ||
                t.TargetVillageId.HasValue && playerVillageIds.Contains(t.TargetVillageId.Value)))
            .ToListAsync(ct);
}
