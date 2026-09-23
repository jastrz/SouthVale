namespace TownManager.Application.Dtos;

public record WorldStatusDto(
    int Iteration,
    DateTime? StartedAt,
    DateTime? EndsAt,
    PreviousWorldIterationDto? Previous,
    BestEverWorldWinnerDto? BestEver
);

public record PreviousWorldIterationDto(
    int Iteration,
    DateTime EndedAt,
    IReadOnlyList<WorldWinnerDto> Winners
);

public record WorldWinnerDto(
    string Username,
    int Score
);

public record BestEverWorldWinnerDto(
    int Iteration,
    string Username,
    int Score
);
