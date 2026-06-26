using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities;

public class Report : Entity
{
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public ReportType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
