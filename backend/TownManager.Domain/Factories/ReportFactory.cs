using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Factories;

public static class ReportFactory
{
    private static string TroopBreakdown(Troops t)
    {
        var parts = new List<string>();
        foreach (TroopType type in Enum.GetValues<TroopType>())
        {
            var count = t.Get(type);
            if (count > 0) parts.Add($"{count} {type}");
        }
        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }

    private static string LootLine(Resources loot) =>
        loot.IsEmpty() ? "" : $"\n\nLoot: {(int)loot.Wood} Wood, {(int)loot.Clay} Clay, {(int)loot.Iron} Iron, {(int)loot.Beer} Beer.";

    private static string GroupLine(string label, Troops t) =>
        t.IsEmpty() ? $"{label}: none" : $"{label}: {TroopBreakdown(t)}";

    private static string LostLine(Troops started, Troops survived)
    {
        var lost = new List<string>();
        foreach (TroopType t in Enum.GetValues<TroopType>())
        {
            var diff = started.Get(t) - survived.Get(t);
            if (diff > 0) lost.Add($"{diff} {t}");
        }
        return lost.Count > 0 ? $"Lost: {string.Join(", ", lost)}" : "Lost: none";
    }

    private static string CombatBody(Troops attackers, Troops attackerSurvivors, Troops defenders, Troops defenderSurvivors, Resources loot) =>
        $"Attackers:\n{GroupLine("Sent", attackers)}\n{LostLine(attackers, attackerSurvivors)}\n{GroupLine("Survived", attackerSurvivors)}\n\n"
        + $"Defenders:\n{GroupLine("Sent", defenders)}\n{LostLine(defenders, defenderSurvivors)}\n{GroupLine("Survived", defenderSurvivors)}"
        + LootLine(loot);

    public static Report AttackReport(Guid playerId, string sourceVillage, string sourcePlayer,
        string targetVillage, string targetPlayer,
        Troops attackers, Troops attackerSurvivors, Troops defenders, Troops defenderSurvivors, Resources loot)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Attack,
            Title = $"Attack on {targetVillage}",
            Body = $"Source: {sourceVillage} ({sourcePlayer})\nTarget: {targetVillage} ({targetPlayer})\n\n"
                   + CombatBody(attackers, attackerSurvivors, defenders, defenderSurvivors, loot),
        };
    }

    public static Report DefenseReport(Guid playerId, string villageName,
        string attackerVillage, string attackerPlayer,
        Troops attackers, Troops attackerSurvivors, Troops defenders, Troops defenderSurvivors, Resources looted)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Defense,
            Title = $"Defense of {villageName}",
            Body = $"Attacker: {attackerVillage} ({attackerPlayer})\nDefender: {villageName}\n\n"
                   + CombatBody(attackers, attackerSurvivors, defenders, defenderSurvivors, looted),
        };
    }

    public static Report ReturnReport(Guid playerId, string villageName,
        string fromVillage, string fromPlayer,
        Troops troops, Resources? loot)
    {
        var lootStr = loot is not null && !loot.IsEmpty()
            ? $" with {(int)loot.Wood} Wood, {(int)loot.Clay} Clay, {(int)loot.Iron} Iron, {(int)loot.Beer} Beer"
            : "";

        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Return,
            Title = $"Return to {villageName}",
            Body = $"From: {fromVillage} ({fromPlayer})\n\n{TroopBreakdown(troops)} returned{lootStr}.",
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

    public static Report AttackCancelledReport(Guid playerId, string sourceVillage, string targetName)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.AttackCancelled,
            Title = $"Attack cancelled — target destroyed",
            Body = $"Source: {sourceVillage}\nTarget: {targetName}\n\nTarget was destroyed before the attack arrived. Troops returned home.",
        };
    }

    public static Report StarvationReport(Guid playerId, string villageName, Troops starved)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Starvation,
            Title = $"Troops fled in {villageName}",
            Body = $"{TroopBreakdown(starved)} fled from village due to beer shortage.",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Report TransportReport(Guid playerId, string fromVillage, string toVillage,
        Troops troops, Resources resources)
    {
        var parts = new List<string>();
        if (!troops.IsEmpty()) parts.Add(TroopBreakdown(troops));
        if (!resources.IsEmpty()) parts.Add($"{(int)resources.Wood} Wood, {(int)resources.Clay} Clay, {(int)resources.Iron} Iron, {(int)resources.Beer} Beer");

        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Transport,
            Title = $"Transport arrived at {toVillage}",
            Body = $"From: {fromVillage}\n\n{string.Join(" and ", parts)} delivered.",
        };
    }
}
