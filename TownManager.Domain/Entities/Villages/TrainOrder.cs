using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

/// <summary>
/// Queued training of a batch of troops; Completed counts units finished out of Amount by CompletesAt.
/// </summary>
public class TrainOrder : Entity
{
    public TroopType Type { get; set; }
    public int Amount { get; set; }
    public int Completed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletesAt { get; set; }

    public Guid VillageId { get; set; }
    public Village Village { get; set; } = null!;
}