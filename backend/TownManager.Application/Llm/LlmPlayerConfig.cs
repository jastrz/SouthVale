namespace TownManager.Application.Llm;

// Config stored in dotnet secrets currently
public class LlmPlayerConfig
{
    public const string SectionName = "LlmPlayer";

    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public string TickIntervalCron { get; set; } = "*/15 * * * *";
    public int MaxActionsPerTick { get; set; } = 20;
    public bool EnableThinking { get; set; } = true;
    public int ThinkingTokens { get; set; } = 1024 * 16;
    public int NonThinkingTokens { get; set; } = 1024 * 4;
    public bool LogActionsConsole { get; set; } = true;
    public bool LogActionsFile { get; set; } = true;
}
