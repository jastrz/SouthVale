namespace TownManager.Domain.Entities;

/// <summary>
/// Mutable bundle of the four primary resources (wood, clay, iron, crop) with add/subtract helpers.
/// </summary>
public class Resources
{
    public double Wood { get; private set; }
    public double Clay { get; private set; }
    public double Iron { get; private set; }
    public double Crop { get; private set; }

    public static Resources Zero => new();

    public Resources(double wood, double clay, double iron, double crop)
    {
        Wood = wood; Clay = clay; Iron = iron; Crop = crop;
    }

    private Resources() { }

    public Resources Add(Resources other) =>
        new(Wood + other.Wood, Clay + other.Clay, Iron + other.Iron, Crop + other.Crop);

    public Resources Subtract(Resources other) =>
        new(Wood - other.Wood, Clay - other.Clay, Iron - other.Iron, Crop - other.Crop);

    public Resources Multiply(double m) =>
        new(Wood * m, Clay * m,
            Iron * m, Crop * m);
    public bool CanAfford(Resources cost) =>
        Wood >= cost.Wood && Clay >= cost.Clay && Iron >= cost.Iron && Crop >= cost.Crop;

    public bool IsEmpty() => Wood == 0 && Clay == 0 && Iron == 0 && Crop == 0;
}