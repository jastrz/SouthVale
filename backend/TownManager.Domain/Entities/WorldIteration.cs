namespace TownManager.Domain.Entities;

/// <summary>
/// One world lifecycle. A soft reset closes the current iteration and opens the next.
/// </summary>
public class WorldIteration : Entity
{
    public int Number { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<WorldIterationWinner> Winners { get; set; } = [];
}

/// <summary>
/// Final leaderboard standing of a player in a finished iteration; list order is rank.
/// </summary>
public class WorldIterationWinner
{
    public string Username { get; set; } = "";
    public int Score { get; set; }
}
