namespace TownManager.Domain.Entities;

/// <summary>
/// Troops currently stationed in a village for defense; TotalCount is the sum across troop types.
/// </summary>
public class Troops
{
    public int Swordsmen { get; set; } = 0;
    public int Archers { get; set; } = 0;

    public int TotalCount => Swordsmen + Archers;
    public static Troops Zero => new();
}