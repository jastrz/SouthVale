using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

// Config stored in dotnet secrets currently
public class LlmPlayerConfig
{
    public const string SectionName = "LlmPlayer";

    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public string TickIntervalCron { get; set; } = "*/15 * * * *";
    public int MaxActionsPerTick { get; set; } = 10;

    public List<BotSeed> Seeds { get; set; } = [];

    public class BotSeed
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public BotPersonality Personality { get; set; }
        public string VillageName { get; set; } = "";
    }
}
