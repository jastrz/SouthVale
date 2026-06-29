using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Interfaces;

public interface IMovementRepository
{
    Task<TroopMovement?> GetByIdAsync(Guid movementId, CancellationToken ct = default);
    Task<IReadOnlyList<TroopMovement>> GetInFlightForPlayerAsync(Guid playerId, IReadOnlyList<Guid> playerVillageIds, CancellationToken ct = default);
}