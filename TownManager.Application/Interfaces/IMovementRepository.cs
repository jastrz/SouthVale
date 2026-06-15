using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Interfaces;

public interface IMovementRepository
{
    Task<TroopMovement?> GetByIdAsync(Guid movementId, CancellationToken ct = default);
}