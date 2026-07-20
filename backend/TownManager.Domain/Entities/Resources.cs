namespace TownManager.Domain.Entities;

/// <summary>
/// Mutable bundle of the four primary resources (wood, clay, iron, beer) with add/subtract helpers.
/// 
/// Uses double instead of int to accumulate fractional production
/// during resource ticks without losing precision over time.
/// </summary>
public class Resources
{
    public double Wood { get; private set; }
    public double Clay { get; private set; }
    public double Iron { get; private set; }
    public double Beer { get; private set; }

    public static Resources Zero => new();

    public Resources(double wood, double clay, double iron, double beer)
    {
        Wood = wood; Clay = clay; Iron = iron; Beer = beer;
    }

    private Resources() { }

    public Resources Add(Resources other) =>
        new(Wood + other.Wood, Clay + other.Clay, Iron + other.Iron, Beer + other.Beer);

    public Resources Subtract(Resources other) =>
        new(Wood - other.Wood, Clay - other.Clay, Iron - other.Iron, Beer - other.Beer);

    public Resources Multiply(double m) =>
        new(Wood * m, Clay * m,
            Iron * m, Beer * m);
    public bool CanAfford(Resources cost) =>
        Wood >= cost.Wood && Clay >= cost.Clay && Iron >= cost.Iron && Beer >= cost.Beer;

    public bool IsEmpty() => Wood == 0 && Clay == 0 && Iron == 0 && Beer == 0;
}