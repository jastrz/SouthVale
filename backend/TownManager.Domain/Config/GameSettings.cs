using TownManager.Domain.Entities;

namespace TownManager.Domain.Config;

// Default multiplier values used for tests, runtime defaults are kept in .env
public static class GameSettings
{
    public const int MapSize = 60;
    public const int MaxVillagesPerPlayer = 8;
    public static int MaxBuildQueueSize { get; set; } = 4;
    public static bool BotsIgnoreQueueSize { get; set; } = false;
    public static Resources StartingResources => new(1800, 1800, 1800, 1800);
    public static float TravelSpeedMultiplier { get; set; } = 1f;
    public static float ResourcesProductionMultiplier { get; set; } = 1f;
    public static float UpkeepMultiplier { get; set; } = 1f;
    public static float BuildSpeedMultiplier { get; set; } = 1f;
    public static float TrainSpeedMultiplier { get; set; } = 1f;
    public static int ConfigVersion { get; set; }
}