using TownManager.Domain.Config;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities;

/// <summary>
/// Troop counts for a single village or army group.
/// Immutable by design — use Add/Subtract to produce new instances.
/// </summary>
public class Troops
{
    public int Swordsmen { get; private set; }
    public int Archers { get; private set; }

    public int TotalCount => Swordsmen + Archers;
    
    public static Troops Zero => new();

    public Troops(int swordsmen, int archers)
    {
        Swordsmen = swordsmen;
        Archers = archers;
    }

    private Troops() { }

    public Troops Add(Troops other) =>
        new(Swordsmen + other.Swordsmen, Archers + other.Archers);
    
    public Troops Add(TroopType type, int count) => type switch
    {
        TroopType.Swordsman => new Troops(Swordsmen + count, Archers),
        TroopType.Archer    => new Troops(Swordsmen, Archers + count),
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown troop type: {type}")
    };

    public Troops Subtract(Troops other) =>
        new(Swordsmen - other.Swordsmen, Archers - other.Archers);

    public bool HasEnough(Troops required) =>
        Swordsmen >= required.Swordsmen && Archers >= required.Archers;

    public bool IsEmpty() => Swordsmen == 0 && Archers == 0;
    
    public Resources CalculateUpkeep() =>
        new(0, 0, 0,
            Swordsmen * TroopsConfig.Get(TroopType.Swordsman).Stats.Upkeep +
            Archers   * TroopsConfig.Get(TroopType.Archer).Stats.Upkeep);
}