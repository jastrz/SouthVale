namespace TownManager.Application.Llm;

// In dev, prefer user-secrets for ApiKey: `dotnet user-secrets set "LlmPlayer:ApiKey" ...`
public class LlmPlayerConfig
{
    public const string SectionName = "LlmPlayer";

    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public string TickIntervalCron { get; set; } = "*/45 * * * *";
    public int MaxActionsPerTick { get; set; } = 50;
    public bool EnableThinking { get; set; } = true;
    public double Temperature { get; set; } = 0.7;
    public int ThinkingTokens { get; set; } = 1024 * 16;
    public int NonThinkingTokens { get; set; } = 1024 * 4;
    public string ReasoningEffort { get; set; } = "medium";
    public int VisibilityRadius { get; set; } = 40;
    public bool LogActionsConsole { get; set; } = true;
    public bool LogActionsFile { get; set; } = true;
}
