using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

/// <summary>
/// Queued construction of a building up to a target level; CompletesAt drives the finish tick.
/// </summary>
public class BuildOrder : Entity
{
    public Guid VillageId { get; set; }
    public Village Village { get; set; } = null!;
    public BuildingType BuildingType { get; set; }
    public int TargetLevel { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime CompletesAt { get; set; }
    
    public string? JobId { get; set; } 

    public static BuildOrder Create(BuildingType type, int targetLevel, TimeSpan duration) => new()
    {
        BuildingType = type,
        TargetLevel = targetLevel,
        StartsAt = DateTime.UtcNow,
        CompletesAt = DateTime.UtcNow.Add(duration)
    };
}