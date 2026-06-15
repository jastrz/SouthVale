using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Infrastructure.Persistence.Repositories;

public class MovementRepository(AppDbContext db) : IMovementRepository
{
    public Task<TroopMovement?> GetByIdAsync(Guid movementId, CancellationToken ct = default) =>
        db.TroopMovements.Include(t => t.Troops)
            .Include(t => t.CarriedResources)
            .FirstOrDefaultAsync(m => m.Id == movementId, ct);
}