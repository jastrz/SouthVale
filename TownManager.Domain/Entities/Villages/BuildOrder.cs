using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

/// <summary>
/// Queued construction of a building up to a target level; CompletesAt drives the finish tick.
/// </summary>
public class BuildOrder : Entity
{
    public BuildingType Type { get; set; }
    public int TargetLevel { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletesAt { get; set; }

    public Guid VillageId { get; set; }
    public Village Village { get; set; } = null!;
}
