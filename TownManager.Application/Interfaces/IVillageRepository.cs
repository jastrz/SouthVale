using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Interfaces;

public interface IVillageRepository
{
    // By villageId
    Task<Village?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithBuildingsAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithActiveOrdersAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetForCombatAsync(Guid id, CancellationToken ct = default);
    
    // By Hangire orderId

    Task<Village?> GetWithBuildingsAndOrdersAsync(Guid orderId, CancellationToken ct);
    Task<Village?> GetWithTrainOrdersAsync(Guid orderId, CancellationToken ct);
    
    // By playerId
    Task<IReadOnlyList<Village>> GetByPlayerAsync(Guid playerId, CancellationToken ct = default);
    
    void Add(Village village);
}
