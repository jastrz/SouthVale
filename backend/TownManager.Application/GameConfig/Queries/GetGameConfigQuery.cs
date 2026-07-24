using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.GameConfig.Queries;

public record GetGameConfigQuery : IRequest<Result<GameConfigDto>>;

public record BuildingLevelConfigDto(
    int Level,
    ResourcesDto UpgradeCost,
    TimeSpan UpgradeTime,
    int WarehouseCapacity,
    int GranaryCapacity,
    ResourcesDto? ProductionPerHour,
    double DefenseMultiplier,
    int CrannyCapacity,
    double TradeRate,
    double BuildSpeedMultiplier,
    double BarracksTrainingSpeed,
    double StableTrainingSpeed,
    double BarracksAttackMultiplier,
    double StableAttackMultiplier
);

public record TroopConfigDto(
    string Type,
    ResourcesDto TrainingCost,
    TimeSpan TrainingTime,
    int Attack,
    int Defense,
    int CarryCapacity,
    int Speed,
    string TrainedAt,
    double Upkeep
);

public record GameConfigDto(
    Dictionary<string, List<BuildingLevelConfigDto>> Buildings,
    Dictionary<string, TroopConfigDto> Troops,
    int MaxVillagesPerPlayer,
    float TravelSpeedMultiplier
);
