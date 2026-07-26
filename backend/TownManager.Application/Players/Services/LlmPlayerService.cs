using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.GameConfig.Queries;
using TownManager.Application.Interfaces;
using TownManager.Application.Llm;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Players.Services;

// Logging terminology:
//   tick  = one full service cycle (processes all bots)
//   pass  = one bot's LLM call + action execution
//   action = single command sent to the game (build, train, attack, etc.)
public class LlmPlayerService(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo,
    IMovementRepository movementRepo,
    IMediator mediator,
    LlmPlayerConfig config,
    ILlmApiClient llmApi,
    ILogger<LlmPlayerService> logger) : ILlmPlayerService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
    };
    
    private static int _totalTicks;
    private static int _totalReceived;
    private static int _totalSucceeded;
    private static int _totalFailed;
    private static int _totalTickErrors;
    private static double _totalSeconds;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            logger.LogWarning("LLM_API_KEY not configured, skipping LLM player tick");
            return;
        }

        var tickNum = _totalTicks + 1;
        LlmActivity.Log?.Invoke("system", $"─── Tick #{tickNum} ───", config.Model,
            new { time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") });

        var bots = await playerRepo.GetBotPlayersAsync(ct);
        logger.LogInformation("LLM tick: processing {Count} bot players", bots.Count);

        var sw = Stopwatch.StartNew();
        var totalReceived = 0;
        var totalSucceeded = 0;
        var totalFailed = 0;
        var totalTickErrors = 0;
        foreach (var bot in bots)
        {
            logger.LogInformation("LLM pass {Username}:", bot.Username);
            LlmActivity.Log?.Invoke(bot.Username, "pass", config.Model, null);
            try
            {
                var (r, s, f, te) = await ProcessBot(bot, ct);
                totalReceived += r;
                totalSucceeded += s;
                totalFailed += f;
                totalTickErrors += te;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LLM pass {Username} crashed", bot.Username);
                LlmActivity.Log?.Invoke(bot.Username, "pass-crash", config.Model, new { error = ex.Message });
            }
        }
        sw.Stop();

        _totalTicks++;
        _totalReceived += totalReceived;
        _totalSucceeded += totalSucceeded;
        _totalFailed += totalFailed;
        _totalTickErrors += totalTickErrors;
        _totalSeconds += sw.Elapsed.TotalSeconds;

        logger.LogInformation("LLM tick summary: {Received} commands, {Succeeded} ok, {Failed} failed, {TickErrors} tick errors, {Duration}s",
            totalReceived, totalSucceeded, totalFailed, totalTickErrors, sw.Elapsed.TotalSeconds.ToString("F1"));
        logger.LogInformation("LLM runtime: {Ticks} ticks, {Received} commands, {Succeeded} ok, {Failed} failed, {TickErrors} tick errors, {Duration}s total",
            _totalTicks, _totalReceived, _totalSucceeded, _totalFailed, _totalTickErrors, Math.Round(_totalSeconds, 1));
        LlmActivity.Log?.Invoke("system", "tick-summary", config.Model,
            new { duration_s = Math.Round(sw.Elapsed.TotalSeconds, 1), received = totalReceived, succeeded = totalSucceeded, failed = totalFailed, tick_errors = totalTickErrors });
        LlmActivity.Log?.Invoke("system", "runtime-summary", config.Model,
            new { ticks = _totalTicks, duration_s = Math.Round(_totalSeconds, 1), received = _totalReceived, succeeded = _totalSucceeded, failed = _totalFailed, tick_errors = _totalTickErrors });
    }

    private async Task<(int received, int succeeded, int failed, int tickErrors)> ProcessBot(Player bot, CancellationToken ct)
    {
        var villages = await villageRepo.GetWithOrdersByPlayerAsync(bot.Id, ct);
        if (villages.Count == 0) return (0, 0, 0, 0);

        var villageIds = villages.Select(v => v.Id).ToList();
        var movements = await movementRepo.GetInFlightForPlayerAsync(bot.Id, villageIds, ct);
        var incomingAttacks = movements.Count(m => m.TargetVillageId.HasValue &&
            villages.Any(v => v.Id == m.TargetVillageId.Value) && m.Type == MovementType.Attack);

        var nearby = await GetNearbyVillages(villages, ct);
        var prompt = await BuildPrompt(bot, villages, incomingAttacks, nearby, ct);
        LlmActivity.PromptLog?.Invoke(bot.Username, prompt);
        var actions = await CallLlmApi(bot.Username, prompt, ct);

        if (actions is null)
        {
            logger.LogWarning("LLM pass {Username}: API error", bot.Username);
            LlmActivity.Log?.Invoke(bot.Username, "pass-error", config.Model, new { reason = "api-error" });
            return (0, 0, 0, 1);
        }

        if (actions.Length == 0)
        {
            logger.LogWarning("LLM pass {Username}: empty response (parse error or no content)", bot.Username);
            LlmActivity.Log?.Invoke(bot.Username, "pass-error", config.Model, new { reason = "empty-response" });
            return (0, 0, 0, 1);
        }

        LlmActivity.Log?.Invoke(bot.Username, "pass-start", config.Model,
            new { villages = villages.Count, incoming = incomingAttacks });
        logger.LogInformation("LLM pass {Username}: {Count} actions", bot.Username, actions.Length);

        var succeeded = 0;
        var failed = 0;
        foreach (var action in actions)
        {
            var r = await ExecuteAction(bot, action, ct);
            if (r) succeeded++; else failed++;
        }
        logger.LogInformation("LLM pass {Username}: {Received} actions, {Succeeded} ok, {Failed} failed",
            bot.Username, actions.Length, succeeded, failed);
        LlmActivity.Log?.Invoke(bot.Username, "pass-end", config.Model,
            new { received = actions.Length, succeeded, failed });
        return (actions.Length, succeeded, failed, 0);
    }

    private async Task<List<Village>> GetNearbyVillages(IReadOnlyList<Village> villages, CancellationToken ct)
    {
        var ownIds = villages.Select(v => v.Id).ToHashSet();
        var seen = new HashSet<Guid>(ownIds);
        var result = new List<Village>();

        foreach (var v in villages)
        {
            var nearby = await villageRepo.GetForMapWithinRadius(v.Coordinates, 25, ct);
            foreach (var n in nearby)
            {
                if (seen.Add(n.Id))
                    result.Add(n);
            }
        }

        return result;
    }

    private async Task<string> BuildPrompt(Player bot, IReadOnlyList<Village> villages,
        int incomingAttacks, List<Village> nearby, CancellationToken ct)
    {

        var personalityDesc = bot.BotPersonality switch
        {
            BotPersonality.Aggressive => "You prioritize military strength. Train troops and attack weaker neighbors only. Expand through conquest, settling for expansion and develop some economy.",
            BotPersonality.Defensive => "You prioritize defense. Maintain a strong garrison, and only attack when you have overwhelming advantage. Protect your villages and settle new.",
            BotPersonality.Economic => "You prioritize resource production and expansion. Upgrade resource buildings, train settlers, and found new villages. Avoid unnecessary wars, but keep some defense.",
            _ => "Play strategically.",
        };

        var prompt = $"""
You are a player in browser strategy game. Your personality: {bot.BotPersonality}.
{personalityDesc}

=== GAME CONFIG ===
{await GetGameConfigInfo(ct)}

=== YOUR VILLAGES ===
""";

        for (int i = 0; i < villages.Count; i++)
        {
            var v = villages[i];
            var effects = BuildingConfig.AggregateEffects(v.Buildings);
            var current = v.GetCurrentResources(effects);
            var buildings = string.Join(", ", v.Buildings.Select(b => $"{b.Type} lv{b.Level}"));
            var buildOrders = string.Join(", ", v.BuildOrders.Select(o => $"{o.BuildingType}->lv{o.TargetLevel}"));
            var trainOrders = string.Join(", ", v.TrainOrders.Select(o => $"{o.Type} x{o.Amount}"));

            prompt += $"""
Village {i + 1}: "{v.Name}" (ID: {v.Id})
  Location: ({v.Coordinates.X}, {v.Coordinates.Y})
  Resources: {(int)current.Wood}W {(int)current.Clay}C {(int)current.Iron}I {(int)current.Beer}B
  Troops: Swordsman={v.Troops.Get(TroopType.Swordsman)} Archer={v.Troops.Get(TroopType.Archer)} Dogs={v.Troops.Get(TroopType.Dogs)} Horsemen={v.Troops.Get(TroopType.Horsemen)} LlamaRiders={v.Troops.Get(TroopType.LlamaRiders)} Settler={v.Troops.Get(TroopType.Settler)}
  Buildings: {buildings}
  Build queue: {(buildOrders.Length > 0 ? buildOrders : "empty")}
  Train queue: {(trainOrders.Length > 0 ? trainOrders : "empty")}

""";
        }

        prompt += $"=== NEARBY VILLAGES ===\n";

        foreach (var n in nearby.Take(20))
        {
            prompt += $"  ID:{n.Id} \"{n.Name}\" ({n.Coordinates.X},{n.Coordinates.Y}) — troops: {n.Troops.TotalCount} — {(n.Player?.Username ?? "unknown")}\n";
        }

        prompt += $$"""
=== ACTIVITY ===
Incoming attacks: {{incomingAttacks}}

=== AVAILABLE ACTIONS ===
Respond with a JSON array of actions. Each action is an object:
{ "action": "build", "village_id": "guid", "building_type": "ClayPit" }
{ "action": "train", "village_id": "guid", "troops": { "Swordsman": 10, "Archer": 5 } }
{ "action": "attack", "village_id": "guid", "target_village_id": "guid", "troops": { "Swordsman": 20 } }
{ "action": "transport", "village_id": "guid", "target_village_id": "guid", "troops": { "Swordsman": 5 }, "resources": { "wood": 100, "clay": 100, "iron": 100, "beer": 100 } }
{ "action": "settle", "village_id": "guid", "target": { "x": 10, "y": 10 } }

Building types: WoodCutter, ClayPit, IronMine, Brewery, Warehouse, Barracks, Stable, Wall, Cranny, TradePost
Troop types: Swordsman, Archer, Dogs, Horsemen, LlamaRiders, Settler
Combat roles: Swordsman = high attack (offense), Archer = high defense, Dogs = attack/movement speed, Horsemen = high attack/fast cavalry, LlamaRiders = balanced/resources carrier. Settler = founding new villages, does not fight.
Settler cost doubles per existing settler/village (geometric). Base cost in game config. (e.g. 2 villages + 1 settler = 2^(2+1-1) = 4 * baseCost)

RESOURCE RULES:
- Each action costs resources (see building/troop costs in game config).
- You can only spend resources you currently have. Check your village resources before picking actions.
- If you can't afford an action, don't include it. Pick a cheaper alternative instead (or ignore performing any action and wait until resources are produced or troops come back)
- Build orders queue and deduct resources immediately. Plan your budget across all actions.
- Example: if you have 400 wood, a Barracks (200W) + ClayPit lv2 (160W) = 360W total — affordable. Adding IronMine lv2 (200W) would exceed 400W, so skip it.

IMPORTANT:
- Use village_id (GUID) from YOUR VILLAGES section. Use target_village_id (GUID) from NEARBY VILLAGES section. Never use village names as IDs.
- Max {{config.MaxActionsPerTick}} actions per tick.
- Always use existing settlers with settle command.
- Pick different places for settling your villages according to your playstyle and settle at least 2 tiles away.
- Attack wisely - if target village has big population, you should adjust sent troops accordingly.
- Respond with ONLY the JSON array, no other text. Never add any new fields outside of provided game config.

""";

        return prompt;
    }

    private async Task<string> GetGameConfigInfo(CancellationToken ct)
    {
        var cfg = (await mediator.Send(new GetGameConfigQuery(), ct)).Value!;
        var sb = new StringBuilder();
        sb.AppendLine("Buildings:");
        foreach (var (type, levels) in cfg.Buildings)
        {
            sb.Append($"  {type}:");
            foreach (var l in levels)
            {
                var c = l.UpgradeCost;
                sb.Append($" lv{l.Level}({c.Wood}Wood,{c.Clay}Clay,{c.Iron}Iron,{c.Beer}Beer,{l.UpgradeTime.TotalMinutes:F0}m");
                if (l.ProductionPerHour is { } p && p.Wood + p.Clay + p.Iron + p.Beer > 0)
                    sb.Append($" -> {(p.Wood > 0 ? $"+{p.Wood}Wood/h " : "")}{(p.Clay > 0 ? $"+{p.Clay}Clay/h " : "")}{(p.Iron > 0 ? $"+{p.Iron}Iron/h " : "")}{(p.Beer > 0 ? $"+{p.Beer}Beer/h" : "")}".TrimEnd());
                if (l.BarracksTrainingSpeed > 1) sb.Append($" train×{l.BarracksTrainingSpeed}");
                if (l.StableTrainingSpeed > 1) sb.Append($" stable×{l.StableTrainingSpeed}");
                sb.Append(')');
            }
            sb.AppendLine();
        }
        sb.AppendLine("Troops:");
        foreach (var (type, t) in cfg.Troops)
        {
            var c = t.TrainingCost;
            sb.AppendLine($"  {type}: {c.Wood}Wood,{c.Clay}Clay,{c.Iron}Iron,{c.Beer}Beer, {t.TrainingTime.TotalSeconds:F0}s atk={t.Attack} def={t.Defense} speed={t.Speed} carry={t.CarryCapacity}");
        }
        return sb.ToString();
    }

    private async Task<LlmAction[]?> CallLlmApi(string username, string prompt, CancellationToken ct)
    {
        const int maxRetries = 3;
        LlmApiResponse? apiResult = null;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            apiResult = await llmApi.CallAsync(prompt, ct);

            if (apiResult.Success && !string.IsNullOrEmpty(apiResult.Content))
                break;

            logger.LogWarning("LLM API error ({Status}), attempt {Attempt}/{MaxRetries}", apiResult.StatusCode, attempt, maxRetries);
            if (attempt < maxRetries - 1)
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        if (apiResult is null || !apiResult.Success || string.IsNullOrEmpty(apiResult.Content))
        {
            LlmActivity.Log?.Invoke(username, "api-error", config.Model,
                new { status = apiResult?.StatusCode, response = apiResult?.RawBody });
            logger.LogWarning("LLM API error ({Status})", apiResult?.StatusCode);
            return null;
        }

        var content = apiResult.Content;

        var cleaned = StripMarkdown(content);
        var start = cleaned.IndexOf('[');
        var end = cleaned.LastIndexOf(']');
        if (start >= 0 && end > start)
            cleaned = cleaned[start..(end + 1)];

        try
        {
            var actions = JsonSerializer.Deserialize<LlmAction[]>(cleaned, JsonOpts);
            LlmActivity.Log?.Invoke(username, "response", config.Model,
                new { actions_count = actions?.Length ?? 0, response = content });
            return actions ?? [];
        }
        catch (JsonException ex)
        {
            LlmActivity.Log?.Invoke(username, "parse-error", config.Model, new { response = content });
            logger.LogError(ex, "Failed to parse LLM response");
            return [];
        }
    }

    private async Task<bool> ExecuteAction(Player bot, LlmAction action, CancellationToken ct)
    {
        var details = new Dictionary<string, object?> { ["action"] = action.Action, ["village_id"] = action.VillageId };
        Result? result = null;
        try
        {
            switch (action.Action)
            {
                case "build" when action.BuildingType is not null:
                {
                    if (!Enum.TryParse<BuildingType>(action.BuildingType, out var bt))
                    {
                        logger.LogWarning("LLM action {User}: unknown building type '{Type}'", bot.Username, action.BuildingType);
                        return false;
                    }
                    details["building"] = action.BuildingType;
                    result = await mediator.Send(new CreateBuildOrderCommand(action.VillageId, bt), ct);
                    break;
                }
                case "train" when action.Troops is not null:
                {
                    var entries = ParseTroops(action.Troops);
                    if (entries.Count == 0)
                    {
                        logger.LogWarning("LLM action {User}: train with no valid troop types", bot.Username);
                        return false;
                    }
                    details["troops"] = action.Troops;
                    result = await mediator.Send(new CreateTrainOrderCommand(action.VillageId, entries), ct);
                    break;
                }
                case "attack" when action.Troops is not null && action.TargetVillageId.HasValue:
                {
                    var entries = ParseTroops(action.Troops);
                    if (entries.Count == 0)
                    {
                        logger.LogWarning("LLM action {User}: attack with no valid troop types", bot.Username);
                        return false;
                    }
                    details["troops"] = action.Troops;
                    details["target"] = action.TargetVillageId;
                    result = await mediator.Send(new CreateAttackOrderCommand(action.VillageId, entries, action.TargetVillageId.Value), ct);
                    break;
                }
                case "transport" when action.Troops is not null && action.TargetVillageId.HasValue:
                {
                    var entries = ParseTroops(action.Troops);
                    var res = action.Resources ?? new ResourcesDto(0, 0, 0, 0);
                    details["troops"] = action.Troops;
                    details["target"] = action.TargetVillageId;
                    details["resources"] = res;
                    result = await mediator.Send(new CreateTransportOrderCommand(action.VillageId, action.TargetVillageId.Value, entries,
                        new ResourcesDto(res.Wood, res.Clay, res.Iron, res.Beer)), ct);
                    break;
                }
                case "settle" when action.Target is not null:
                {
                    details["target_coords"] = action.Target;
                    result = await mediator.Send(new CreateSettleOrderCommand(action.VillageId,
                        new Coordinates(action.Target.X, action.Target.Y)), ct);
                    break;
                }
                default:
                    logger.LogWarning("LLM action {User}: unknown action '{Action}'", bot.Username, action.Action);
                    return false;
            }
        }
        catch (Exception ex)
        {
            details["error"] = ex.Message;
            if (config.LogActionsConsole)
                logger.LogWarning(ex, "LLM action {User}: {Action} failed: {@Details}", bot.Username, action.Action, details);
            if (config.LogActionsFile)
                LlmActivity.Log?.Invoke(bot.Username, "action-error", config.Model, details);
            return false;
        }

        if (result is null) return false;
        if (result.Succeeded)
        {
            if (config.LogActionsConsole)
                logger.LogInformation("LLM action {User}: {Action}: {@Details}", bot.Username, action.Action, details);
            if (config.LogActionsFile)
                LlmActivity.Log?.Invoke(bot.Username, "action-success", config.Model, details);
            return true;
        }
        else
        {
            details["errors"] = result.Errors;
            if (config.LogActionsConsole)
                logger.LogWarning("LLM action {User}: {Action} rejected: {@Details}", bot.Username, action.Action, details);
            if (config.LogActionsFile)
                LlmActivity.Log?.Invoke(bot.Username, "action-failure", config.Model, details);
            return false;
        }
    }

    private static List<TroopEntry> ParseTroops(Dictionary<string, int> dict) =>
        dict.Where(kv => kv.Value > 0)
            .Select(kv => (success: Enum.TryParse<TroopType>(kv.Key, out var tt), tt, kv.Value))
            .Where(x => x.success)
            .Select(x => new TroopEntry(x.tt, x.Value))
            .ToList();

    private static string StripMarkdown(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```"))
        {
            var first = text.IndexOf('\n');
            if (first >= 0)
                text = text[(first + 1)..];
        }
        if (text.EndsWith("```"))
            text = text[..^3];
        return text.Trim();
    }

}

public record LlmAction
{
    [JsonPropertyName("action")]
    public string Action { get; init; } = "";

    [JsonPropertyName("village_id")]
    public Guid VillageId { get; init; }

    [JsonPropertyName("building_type")]
    public string? BuildingType { get; init; }

    [JsonPropertyName("target_village_id")]
    public Guid? TargetVillageId { get; init; }

    [JsonPropertyName("troops")]
    public Dictionary<string, int>? Troops { get; init; }

    [JsonPropertyName("resources")]
    public ResourcesDto? Resources { get; init; }

    [JsonPropertyName("target")]
    public CoordinatesDto? Target { get; init; }
}

public record CoordinatesDto(int X, int Y);
