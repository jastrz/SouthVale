using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

public record MovementSchedule(Guid MovementId, TimeSpan Delay);

/// <summary>
/// A queued attack order targeting another village.
/// </summary>
public class TroopMovement : Entity
{
    public Troops Troops { get; set; } = Troops.Zero;
    public Resources? CarriedResources { get; set; }
    
    // for attack/transfer — existing village
    public Guid? TargetVillageId { get; set; }
    
    // for settle — empty tile
    public Coordinates? TargetCoordinates { get; set; }
    
    public MovementType Type { get; set; }
    public MovementStatus Status { get; set; }
    
    public DateTime DepartureAt { get; set; }
    public DateTime ArrivesAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid VillageId { get; set; }
    public Village Village { get; set; } = null!;

    public static TroopMovement Create(Troops troops, Guid targetVillageId, TimeSpan travelTime, 
        DateTime departureAt, MovementType movementType)
    {
        return new()
        {
            Troops = troops,
            TargetVillageId = targetVillageId,
            DepartureAt = departureAt,
            ArrivesAt = departureAt.Add(travelTime),
            Type = movementType,
            Status = MovementStatus.InFlight
        };
    }
    
    public static TroopMovement Create(Troops troops, Resources resources, Guid targetVillageId, TimeSpan travelTime,
        DateTime departureAt, MovementType movementType)
    {
        return new()
        {
            Troops = troops,
            TargetVillageId = targetVillageId,
            DepartureAt = departureAt,
            ArrivesAt = departureAt.Add(travelTime),
            Type = movementType,
            CarriedResources = resources,
            Status = MovementStatus.InFlight
        };
    }

    public static TroopMovement CreateSettle(Troops troops, Coordinates target,
        TimeSpan travelTime, DateTime departureAt)
    {
        return new()
        {
            Troops = troops,
            TargetCoordinates = target,
            DepartureAt = departureAt,
            ArrivesAt = departureAt.Add(travelTime),
            Type = MovementType.Settle,
            Status = MovementStatus.InFlight
        };
    }
}
