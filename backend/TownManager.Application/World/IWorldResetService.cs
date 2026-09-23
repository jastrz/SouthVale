using TownManager.Domain.Entities;

namespace TownManager.Application.World;

public record WorldResetResult(int IterationNumber, int PersistedUsers, int RecreatedPlayers);

public interface IWorldResetService
{
    Task<WorldIteration?> GetCurrentIterationAsync(CancellationToken ct = default);

    /// <summary>
    /// Ends the current iteration, wipes all game data, recreates a starter village for every
    /// registered non-bot user and opens the next iteration.
    /// </summary>
    Task<WorldResetResult> ResetAsync(CancellationToken ct = default);
}
