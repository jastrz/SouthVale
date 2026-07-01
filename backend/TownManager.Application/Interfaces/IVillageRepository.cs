using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Interfaces;

public interface IVillageRepository
{
    Task<IReadOnlyList<Village>> GetForMapWithinRadius(Coordinates? center, int? radius, CancellationToken ct = default);
    
    // By villageId
    Task<Village?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithActiveOrdersAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetForCombatAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithMovementOrdersAsync(Guid id, CancellationToken ct = default);
    // By Hangire orderId

    Task<Village?> GetWithBuildingsAndOrdersAsync(Guid orderId, CancellationToken ct);
    Task<Village?> GetWithTrainOrdersAsync(Guid orderId, CancellationToken ct);
    
    // By playerId
    Task<IReadOnlyList<Village>> GetSummariesByPlayerAsync(Guid playerId, CancellationToken ct = default);
    Task<IReadOnlyList<Village>> GetFullDetailsByPlayerAsync(Guid playerId, CancellationToken ct = default);
    Task<IReadOnlyList<Village>> GetWithOrdersByPlayerAsync(Guid playerId, CancellationToken ct = default);

    // By map coordinates
    Task<Village?> GetByCoordsAsync(Coordinates coordinates, CancellationToken ct = default);

    // Lookups
    Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<int?> GetMaxBuildOrderTargetAsync(Guid villageId, BuildingType type, CancellationToken ct = default);

    void Add(Village village);
    Task SaveChangesAsync(CancellationToken ct = default);
}
