using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Players.Services;

public class LlmPlayerService(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo,
    IMovementRepository movementRepo,
    IMediator mediator,
    LlmPlayerConfig config,
    ILogger<LlmPlayerService> logger) : ILlmPlayerService
{
    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
    };

    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            logger.LogWarning("LLM_API_KEY not configured, skipping LLM player tick");
            return;
        }

        var bots = await playerRepo.GetBotPlayersAsync(ct);
        logger.LogInformation("LLM tick: processing {Count} bot players", bots.Count);

        foreach (var bot in bots)
        {
            try
            {
                await ProcessBot(bot, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LLM tick failed for bot {Username}", bot.Username);
            }
        }
    }

    private async Task ProcessBot(Player bot, CancellationToken ct)
    {
        var villages = await villageRepo.GetWithOrdersByPlayerAsync(bot.Id, ct);
        if (villages.Count == 0) return;

        var villageIds = villages.Select(v => v.Id).ToList();
        var movements = await movementRepo.GetInFlightForPlayerAsync(bot.Id, villageIds, ct);
        var incomingAttacks = movements.Count(m => m.TargetVillageId.HasValue &&
            villages.Any(v => v.Id == m.TargetVillageId.Value) && m.Type == MovementType.Attack);

        var nearby = await GetNearbyVillages(villages, ct);
        var prompt = BuildPrompt(bot, villages, incomingAttacks, nearby);
        LlmActivity.PromptLog?.Invoke(bot.Username, prompt);
        var actions = await CallLlmApi(bot.Username, prompt, ct);

        if (actions is null || actions.Length == 0)
        {
            logger.LogInformation("LLM player {Username}: no actions returned", bot.Username);
            return;
        }

        logger.LogInformation("LLM player {Username}: executing {Count} actions",
            bot.Username, actions.Length);

        foreach (var action in actions)
            await ExecuteAction(bot, villages, action, ct);
    }

    private async Task<List<Village>> GetNearbyVillages(IReadOnlyList<Village> villages, CancellationToken ct)
    {
        var seen = new HashSet<Guid> { };
        var result = new List<Village>();

        foreach (var v in villages)
        {
            var nearby = await villageRepo.GetForMapWithinRadius(v.Coordinates, 15, ct);
            foreach (var n in nearby)
            {
                if (seen.Add(n.Id))
                    result.Add(n);
            }
        }

        return result;
    }

    private string BuildPrompt(Player bot, IReadOnlyList<Village> villages,
        int incomingAttacks, List<Village> nearby)
    {

        var personalityDesc = bot.BotPersonality switch
        {
            BotPersonality.Aggressive => "You prioritize military strength. Train troops and attack weaker neighbors only. Expand through conquest, settling and some economy. ",
            BotPersonality.Defensive => "You prioritize defense. Maintain a strong garrison, and only attack when you have overwhelming advantage. Protect your villages.",
            BotPersonality.Economic => "You prioritize resource production and expansion. Upgrade resource buildings, train settlers, and found new villages. Avoid unnecessary wars.",
            _ => "Play strategically.",
        };

        var prompt = $"""
You are a player in browser strategy game. Your personality: {bot.BotPersonality}.
{personalityDesc}

=== GAME CONFIG ===
{GetGameConfigJson()}

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
  Resources: {(int)current.Wood}W {(int)current.Clay}C {(int)current.Iron}I {(int)current.Crop}Cr
  Troops: {v.Troops.Swordsmen}S {v.Troops.Archers}A {v.Troops.Settlers}St
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
{ "action": "transport", "village_id": "guid", "target_village_id": "guid", "troops": { "Swordsman": 5 }, "resources": { "wood": 100, "clay": 100, "iron": 100, "crop": 100 } }
{ "action": "settle", "village_id": "guid", "target": { "x": 10, "y": 10 } }

Building types: WoodCutter, ClayPit, IronMine, CropField, Warehouse, Granary, Barracks
Troop types: Swordsman, Archer, Settler

RESOURCE RULES:
- Each action costs resources (see building/troop costs in game config).
- You can only spend resources you currently have. Check your village resources before picking actions.
- If you can't afford an action, don't include it. Pick a cheaper alternative instead (or ignore performing any action and wait until resources are produced or troops come back)
- Build orders queue and deduct resources immediately. Plan your budget across all actions.
- Example: if you have 400 wood, a Barracks (200W) + ClayPit lv2 (160W) = 360W total — affordable. Adding IronMine lv2 (200W) would exceed 400W, so skip it.

IMPORTANT:
- Use village_id (GUID) from YOUR VILLAGES section. Use target_village_id (GUID) from NEARBY VILLAGES section. Never use village names as IDs.
- Max {{config.MaxActionsPerTick}} actions per tick.
- Use settle command - it's worth it!
- Respond with ONLY the JSON array, no other text. Never add any new fields outside of provided game config.
""";

        return prompt;
    }

    private string GetGameConfigJson()
    {
        var buildings = BuildingConfig.Levels.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value.Select(l => new
            {
                level = l.Level,
                cost = new { wood = l.UpgradeCost.Wood, clay = l.UpgradeCost.Clay, iron = l.UpgradeCost.Iron, crop = l.UpgradeCost.Crop },
                time_minutes = Math.Round(l.UpgradeTime.TotalMinutes, 1),
                produces = l.Effects.ProductionPerHour,
            }));

        var troops = TroopsConfig.All.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => new
            {
                cost = new { wood = kvp.Value.TrainingCost.Wood, clay = kvp.Value.TrainingCost.Clay, iron = kvp.Value.TrainingCost.Iron, crop = kvp.Value.TrainingCost.Crop },
                time_seconds = kvp.Value.TrainingTime.TotalSeconds,
                attack = kvp.Value.Stats.Attack,
                defense = kvp.Value.Stats.Defense,
                speed = kvp.Value.Stats.Speed,
                upkeep = kvp.Value.Stats.Upkeep,
            });

        return JsonSerializer.Serialize(new { buildings, troops }, JsonOpts);
    }

    private async Task<LlmAction[]?> CallLlmApi(string username, string prompt, CancellationToken ct)
    {
        var body = new
        {
            model = config.Model,
            messages = new[]
            {
                new { role = "system", content = prompt },
            },
            temperature = 0.7,
            max_tokens = 4096,
            thinking = new { type = "disabled" },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{config.ApiUrl.TrimEnd('/')}/v1/chat/completions")
        {
            Content = JsonContent.Create(body, options: JsonOpts),
        };
        request.Headers.Authorization = new("Bearer", config.ApiKey);

        using var response = await Http.SendAsync(request, ct);
        var rawBody = await response.Content.ReadAsStringAsync(ct);
        // logger.LogInformation("LLM raw response ({Status}): {Body}", response.StatusCode, rawBody);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("LLM API error: {Body}", rawBody);
            return null;
        }

        var parsed = JsonSerializer.Deserialize<OpenAiResponse>(rawBody, JsonOpts);
        var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrEmpty(content))
        {
            LlmActivity.Log?.Invoke(username, "empty-response", config.Model, null);
            logger.LogWarning("LLM returned empty response");
            return null;
        }

        // logger.LogInformation("LLM response from {Username}: {Content}", username, content);

        var cleaned = StripMarkdown(content);

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
            logger.LogError(ex, "Failed to parse LLM response: {Content}", content);
            return [];
        }
    }

    private async Task ExecuteAction(Player bot, IReadOnlyList<Village> villages, LlmAction action, CancellationToken ct)
    {
        try
        {
            switch (action.Action)
            {
                case "build" when action.BuildingType is not null:
                {
                    if (!Enum.TryParse<BuildingType>(action.BuildingType, out var bt)) return;
                    await mediator.Send(new CreateBuildOrderCommand(action.VillageId, bt), ct);
                    logger.LogInformation("LLM {User}: build {Bt} in village {VId}", bot.Username, bt, action.VillageId);
                    break;
                }
                case "train" when action.Troops is not null:
                {
                    var entries = action.Troops
                        .Where(t => t.Value > 0)
                        .Select(t => Enum.TryParse<TroopType>(t.Key, out var tt) ? (tt, t.Value) : default)
                        .Where(x => x.tt != default)
                        .Select(x => new TroopEntry(x.tt, x.Value))
                        .ToList();
                    if (entries.Count > 0)
                        await mediator.Send(new CreateTrainOrderCommand(action.VillageId, entries), ct);
                    logger.LogInformation("LLM {User}: trained in village {VId}", bot.Username, action.VillageId);
                    break;
                }
                case "attack" when action.Troops is not null && action.TargetVillageId.HasValue:
                {
                    var entries = action.Troops
                        .Where(t => t.Value > 0)
                        .Select(t => Enum.TryParse<TroopType>(t.Key, out var tt) ? (tt, t.Value) : default)
                        .Where(x => x.tt != default)
                        .Select(x => new TroopEntry(x.tt, x.Value))
                        .ToList();
                    if (entries.Count > 0)
                        await mediator.Send(new CreateAttackOrderCommand(action.VillageId, entries, action.TargetVillageId.Value), ct);
                    logger.LogInformation("LLM {User}: attacked {Target} from {VId}", bot.Username, action.TargetVillageId, action.VillageId);
                    break;
                }
                case "transport" when action.Troops is not null && action.TargetVillageId.HasValue:
                {
                    var entries = action.Troops
                        .Where(t => t.Value > 0)
                        .Select(t => Enum.TryParse<TroopType>(t.Key, out var tt) ? (tt, t.Value) : default)
                        .Where(x => x.tt != default)
                        .Select(x => new TroopEntry(x.tt, x.Value))
                        .ToList();
                    var res = action.Resources ?? new ResourcesDto(0, 0, 0, 0);
                    await mediator.Send(new CreateTransportOrderCommand(action.VillageId, action.TargetVillageId.Value, entries,
                        new ResourcesDto(res.Wood, res.Clay, res.Iron, res.Crop)), ct);
                    logger.LogInformation("LLM {User}: transport from {VId} to {Target}", bot.Username, action.VillageId, action.TargetVillageId);
                    break;
                }
                case "settle" when action.Target is not null:
                {
                    await mediator.Send(new CreateSettleOrderCommand(action.VillageId,
                        new Coordinates(action.Target.X, action.Target.Y)), ct);
                    logger.LogInformation("LLM {User}: settle to ({X},{Y}) from {VId}", bot.Username,
                        action.Target.X, action.Target.Y, action.VillageId);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LLM action failed for {User}: {Action}", bot.Username, action.Action);
        }
    }

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

    private record OpenAiResponse
    {
        public OpenAiChoice[]? Choices { get; init; }
    }

    private record OpenAiChoice
    {
        public OpenAiMessage? Message { get; init; }
    }

    private record OpenAiMessage
    {
        public string? Content { get; init; }
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
