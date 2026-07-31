namespace TownManager.Application.Barbarians;

public class BarbarianOptions
{
    public const string SectionName = "Barbarian";
    public string TickIntervalCron { get; set; } = "*/30 * * * *";
}
