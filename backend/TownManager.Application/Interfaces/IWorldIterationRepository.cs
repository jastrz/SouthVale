using TownManager.Domain.Entities;

namespace TownManager.Application.Interfaces;

public interface IWorldIterationRepository
{
    Task<WorldIteration?> GetCurrentAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WorldIteration>> GetFinishedAsync(CancellationToken ct = default);
}
