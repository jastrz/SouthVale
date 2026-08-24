namespace TownManager.Application.Dtos;

public record EmpireDto(
    int MaxBuildQueueSize,
    double MaxBarracksMultiplier,
    double MaxStableMultiplier
);