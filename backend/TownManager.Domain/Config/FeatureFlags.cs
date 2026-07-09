namespace TownManager.Domain.Config;

public class FeatureFlags
{
    public const string SectionName = "Features";
    public bool UseBarbarians { get; init; } = true;
    public bool UseLlmPlayers { get; init; }
}
