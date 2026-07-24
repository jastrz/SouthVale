using TownManager.Domain.Entities;

namespace TownManager.Domain.Config;

public static class GameSettings
{
    public const int MapSize = 60;
    public const int MaxVillagesPerPlayer = 8;
    public static Resources StartingResources => new(800, 800, 800, 800);
    public static float TravelSpeedMultiplier { get; set; } = 10f;
    public static float ResourcesProductionMultiplier { get; set; } = 16f;
    public static int ConfigVersion { get; set; }
}