namespace TownManager.Application.Dtos;

public record LeaderboardResultDto(IReadOnlyList<LeaderboardEntryDto> Items, int TotalCount);
