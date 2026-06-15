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
    public int Settlers { get; private set; }

    public int TotalCount => Swordsmen + Archers + Settlers;

    public static Troops Zero => new();

    public Troops(int swordsmen, int archers, int settlers = 0)
    {
        Swordsmen = swordsmen;
        Archers = archers;
        Settlers = settlers;
    }

    private Troops() { }

    public Troops Add(Troops other) =>
        new(Swordsmen + other.Swordsmen, Archers + other.Archers, Settlers + other.Settlers);

    public Troops Add(TroopType type, int count) => type switch
    {
        TroopType.Swordsman => new Troops(Swordsmen + count, Archers, Settlers),
        TroopType.Archer    => new Troops(Swordsmen, Archers + count, Settlers),
        TroopType.Settler   => new Troops(Swordsmen, Archers, Settlers + count),
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown troop type: {type}")
    };

    public Troops Subtract(Troops other) =>
        new(Swordsmen - other.Swordsmen, Archers - other.Archers, Settlers - other.Settlers);

    public bool HasEnough(Troops required) =>
        Swordsmen >= required.Swordsmen && Archers >= required.Archers && Settlers >= required.Settlers;

    public bool IsEmpty() => Swordsmen == 0 && Archers == 0 && Settlers == 0;

    public Resources CalculateUpkeep() =>
        new(0, 0, 0,
            Swordsmen * TroopsConfig.Get(TroopType.Swordsman).Stats.Upkeep +
            Archers   * TroopsConfig.Get(TroopType.Archer).Stats.Upkeep +
            Settlers  * TroopsConfig.Get(TroopType.Settler).Stats.Upkeep);
}