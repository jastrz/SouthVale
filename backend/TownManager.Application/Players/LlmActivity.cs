namespace TownManager.Application.Players;

public static class LlmActivity
{
    public static Action<string, string, string, object?>? Log { get; set; }
    public static Action<string, string>? PromptLog { get; set; }
}
