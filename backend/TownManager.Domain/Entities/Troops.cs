using TownManager.Domain.Config;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities;

public class Troops
{
    public Dictionary<TroopType, int> Counts { get; set; } = [];

    public static Troops Zero => new();

    public Troops() { }

    public Troops(int swordsmen = 0, int archers = 0, int settlers = 0,
        int dogs = 0, int horsemen = 0, int llamaRiders = 0)
    {
        if (swordsmen > 0) Counts[TroopType.Swordsman] = swordsmen;
        if (archers > 0) Counts[TroopType.Archer] = archers;
        if (settlers > 0) Counts[TroopType.Settler] = settlers;
        if (dogs > 0) Counts[TroopType.Dogs] = dogs;
        if (horsemen > 0) Counts[TroopType.Horsemen] = horsemen;
        if (llamaRiders > 0) Counts[TroopType.LlamaRiders] = llamaRiders;
    }

    public int TotalCount => Counts.Values.Sum();
    public bool IsEmpty() => Counts.All(kv => kv.Value <= 0);

    public int Get(TroopType t) => Counts.GetValueOrDefault(t);

    public Troops Add(TroopType type, int count)
    {
        var next = new Troops { Counts = new(Counts) { [type] = Get(type) + count } };
        return next;
    }

    public Troops Add(Troops other)
    {
        var d = new Dictionary<TroopType, int>(Counts);
        foreach (var (t, c) in other.Counts)
            d[t] = d.GetValueOrDefault(t) + c;
        return new Troops { Counts = d };
    }

    public Troops Subtract(Troops other)
    {
        var d = new Dictionary<TroopType, int>(Counts);
        foreach (var (t, c) in other.Counts)
            d[t] = Math.Max(0, d.GetValueOrDefault(t) - c);
        return new Troops { Counts = d };
    }

    public Troops Clone() => new Troops { Counts = new(Counts) };
    
    public bool HasEnough(Troops required) =>
        required.Counts.All(kv => Get(kv.Key) >= kv.Value);

    public double GetUpkeepPerHour()
    {
        double total = 0;
        foreach (var (type, count) in Counts)
            total += TroopsConfig.Get(type).Upkeep * count;
        return total;
    }
}
