namespace TownManager.Application.Dtos;

public record LeaderboardEntryDto(
    Guid PlayerId,
    string Username,
    int Score,
    int Rank
);
