using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Interfaces;


public interface IMovementResolver
{
    MovementType Handles { get; }
    Task ResolveAsync(TroopMovement movement, CancellationToken ct);
}
