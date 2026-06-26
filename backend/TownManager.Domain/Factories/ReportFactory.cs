using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Factories;

public static class ReportFactory
{
    public static Report AttackReport(Guid playerId, string targetName, Troops survivors, Resources loot)
    {
        var body = survivors.IsEmpty()
            ? "All troops lost."
            : $"Survivors returning with {(int)loot.Wood} Wood, {(int)loot.Clay} Clay, {(int)loot.Iron} Iron, {(int)loot.Crop} Crop.";

        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Attack,
            Title = $"Attack on {targetName}",
            Body = body,
        };
    }

    public static Report DefenseReport(Guid playerId, string villageName, Troops survivors, Resources looted)
    {
        var body = survivors.IsEmpty()
            ? "No defenders left."
            : $"{survivors.TotalCount} troops survived. {(int)looted.Wood} Wood, {(int)looted.Clay} Clay, {(int)looted.Iron} Iron, {(int)looted.Crop} Crop looted.";

        return new Report
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Type = ReportType.Defense,
            Title = $"Defense of {villageName}",
            Body = body,
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
            Body = $"{troops.TotalCount} troops returned{lootStr}.",
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
