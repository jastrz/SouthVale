using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Interfaces;

public interface IVillageRepository
{
    // By villageId
    Task<Village?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithBuildingsAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithActiveOrdersAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetForCombatAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithMovementOrdersAsync(Guid id, CancellationToken ct = default); 
    
    
    // By Hangire orderId

    Task<Village?> GetWithBuildingsAndOrdersAsync(Guid orderId, CancellationToken ct);
    Task<Village?> GetWithTrainOrdersAsync(Guid orderId, CancellationToken ct);
    Task<Village?> GetWithAttackOrdersAsyncByOrder(Guid orderId, CancellationToken ct = default); 
    
    // By playerId
    Task<IReadOnlyList<Village>> GetSummariesByPlayerAsync(Guid playerId, CancellationToken ct = default);
    Task<IReadOnlyList<Village>> GetFullDetailsByPlayerAsync(Guid playerId, CancellationToken ct = default);

    // By map coordinates
    Task<Village?> GetByCoordsAsync(Coordinates coordinates, CancellationToken ct = default);

    // Lookups
    Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    void Add(Village village);
}
