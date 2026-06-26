using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Factories;

public static class ReportFactory
{
    private static string TroopBreakdown(Troops t)
    {
        var parts = new List<string>();
        if (t.Swordsmen > 0) parts.Add($"{t.Swordsmen} Swordsmen");
        if (t.Archers > 0) parts.Add($"{t.Archers} Archers");
        if (t.Settlers > 0) parts.Add($"{t.Settlers} Settlers");
        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }

    private static string LootLine(Resources loot) =>
        loot.IsEmpty() ? "" : $"\nLoot: {(int)loot.Wood} Wood, {(int)loot.Clay} Clay, {(int)loot.Iron} Iron, {(int)loot.Crop} Crop.";

    private static string CombatBody(Troops attackers, Troops attackerSurvivors, Troops defenders, Troops defenderSurvivors, Resources loot) =>
        $"Attackers: {TroopBreakdown(attackerSurvivors)} of {TroopBreakdown(attackers)} survived.\n"
        + $"Defenders: {TroopBreakdown(defenderSurvivors)} of {TroopBreakdown(defenders)} survived."
        + LootLine(loot);

    public static Report AttackReport(Guid playerId, string targetName, Troops attackers, Troops attackerSurvivors, Troops defenders, Troops defenderSurvivors, Resources loot)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Attack,
            Title = $"Attack on {targetName}",
            Body = CombatBody(attackers, attackerSurvivors, defenders, defenderSurvivors, loot),
        };
    }

    public static Report DefenseReport(Guid playerId, string villageName, Troops attackers, Troops attackerSurvivors, Troops defenders, Troops defenderSurvivors, Resources looted)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Defense,
            Title = $"Defense of {villageName}",
            Body = CombatBody(attackers, attackerSurvivors, defenders, defenderSurvivors, looted),
        };
    }

    public static Report ReturnReport(Guid playerId, string villageName, Troops troops, Resources? loot)
    {
        var lootStr = loot is not null && !loot.IsEmpty()
            ? $" with {(int)loot.Wood} Wood, {(int)loot.Clay} Clay, {(int)loot.Iron} Iron, {(int)loot.Crop} Crop"
            : "";

        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Return,
            Title = $"Return to {villageName}",
            Body = $"{TroopBreakdown(troops)} returned{lootStr}.",
        };
    }

    public static Report SettleReport(Guid playerId, string villageName, int x, int y)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Settle,
            Title = "New settlement established",
            Body = $"Village \"{villageName}\" founded at ({x}, {y}).",
        };
    }

    public static Report SettleFailedReport(Guid playerId, int x, int y)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Settle,
            Title = "Settlement failed",
            Body = $"Tile ({x}, {y}) was already occupied. Settlers returning.",
        };
    }
}
