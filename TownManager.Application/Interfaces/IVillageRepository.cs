using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Interfaces;

public interface IVillageRepository
{
    Task<Village?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Aggregate-loaded queries — the value of having a repo
    Task<Village?> GetWithBuildingsAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetWithActiveOrdersAsync(Guid id, CancellationToken ct = default);
    Task<Village?> GetForCombatAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Village>> GetByPlayerAsync(Guid playerId, CancellationToken ct = default);
    Task<IReadOnlyList<Village>> GetVillagesCompletingBeforeAsync(DateTime cutoff, CancellationToken ct = default);

    void Add(Village village);
}
