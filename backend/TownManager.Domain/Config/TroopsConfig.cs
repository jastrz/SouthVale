using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

public record TroopStats
{
    public int Attack { get; init; }
    public int Defense { get; init; }
    public int CarryCapacity { get; init; }
    public int Speed { get; init; }        // fields per hour
    public int Upkeep { get; init; }       // crop consumption per hour

    public TroopStats(
        int attack,
        int defense,
        int carryCapacity,
        int speed,
        int upkeep)
    {
        Attack = attack;
        Defense = defense;
        CarryCapacity = carryCapacity;
        Speed = speed;
        Upkeep = upkeep;
    }
}

public record TroopConfig(
    TroopType Type,
    Resources TrainingCost,
    TimeSpan TrainingTime,
    TroopStats Stats
);

public static class TroopsConfig
{
    public static readonly Dictionary<TroopType, TroopConfig> All = new()
    {
        [TroopType.Swordsman] = new(
            TroopType.Swordsman,
            // new Resources(120, 100, 150, 30),
            new Resources(1,1,1,1),
            
            TimeSpan.FromSeconds(10),
            new TroopStats(attack: 60, defense: 30, carryCapacity: 50, speed: 6, upkeep: 1)
        ),

        [TroopType.Archer] = new(
            TroopType.Archer,
            new Resources(1,1,1,1),

            // new Resources(80, 60, 120, 40),
            TimeSpan.FromSeconds(10),
            new TroopStats(attack: 45, defense: 50, carryCapacity: 30, speed: 7, upkeep: 1)
        ),

        [TroopType.Settler] = new(
            TroopType.Settler,
            new Resources(100, 100, 100, 100),
            TimeSpan.FromSeconds(20),
            new TroopStats(attack: 0, defense: 0, carryCapacity: 0, speed: 5, upkeep: 0)
        ),
    };

    public static TroopConfig Get(TroopType type) => All[type];

    public static TimeSpan CalculateTrainingTime(TroopType type, int count, double speedMultiplier = 1.0) =>
        All[type].TrainingTime * count / speedMultiplier;

    public static int CalculateTotalUpkeep(IEnumerable<(TroopType Type, int Count)> troops) =>
        troops.Sum(t => All[t.Type].Stats.Upkeep * t.Count);

    public static int GetSlowestSpeed(Troops troops)
    {
        var speeds = new List<int>();
        if (troops.Swordsmen > 0) speeds.Add(All[TroopType.Swordsman].Stats.Speed);
        if (troops.Archers > 0) speeds.Add(All[TroopType.Archer].Stats.Speed);
        if (troops.Settlers > 0) speeds.Add(All[TroopType.Settler].Stats.Speed);
        return speeds.Min();
    }
}