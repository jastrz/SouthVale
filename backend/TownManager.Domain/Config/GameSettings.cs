using TownManager.Domain.Entities;

namespace TownManager.Domain.Config;

public static class GameSettings
{
    public const int MapSize = 60;
    public const int MaxVillagesPerPlayer = 8;
    public static readonly Resources StartingResources = new Resources(800,800,800,800);
}