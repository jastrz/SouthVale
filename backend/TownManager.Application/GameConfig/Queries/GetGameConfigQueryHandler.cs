using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Domain.Config;

namespace TownManager.Application.GameConfig.Queries;

public class GetGameConfigQueryHandler : IRequestHandler<GetGameConfigQuery, Result<GameConfigDto>>
{
    private static readonly GameConfigDto Config = BuildConfig();

    public Task<Result<GameConfigDto>> Handle(GetGameConfigQuery query, CancellationToken ct) =>
        Task.FromResult(Result<GameConfigDto>.Success(Config));

    private static GameConfigDto BuildConfig()
    {
        var buildings = BuildingConfig.Levels.ToDictionary(
            b => b.Key.ToString(),
            b => b.Value.Select(l => new BuildingLevelConfigDto(
                l.Level,
                new ResourcesDto((int)l.UpgradeCost.Wood, (int)l.UpgradeCost.Clay, (int)l.UpgradeCost.Iron, (int)l.UpgradeCost.Beer),
                l.UpgradeTime,
                l.Effects.WarehouseCapacity,
                l.Effects.GranaryCapacity,
                l.Effects.ProductionPerHour.IsEmpty() ? null : new ResourcesDto(
                    (int)l.Effects.ProductionPerHour.Wood,
                    (int)l.Effects.ProductionPerHour.Clay,
                    (int)l.Effects.ProductionPerHour.Iron,
                    (int)l.Effects.ProductionPerHour.Beer
                ),
                l.Effects.TrainingSpeedMultiplier,
                l.Effects.DefenseMultiplier,
                l.Effects.CrannyCapacity,
                l.Effects.TradeRate,
                l.Effects.BuildSpeedMultiplier
            )).ToList()
        );

        var troops = TroopsConfig.All.ToDictionary(
            t => t.Key.ToString(),
            t => new TroopConfigDto(
                t.Key.ToString(),
                new ResourcesDto((int)t.Value.TrainingCost.Wood, (int)t.Value.TrainingCost.Clay, (int)t.Value.TrainingCost.Iron, (int)t.Value.TrainingCost.Beer),
                t.Value.TrainingTime,
                t.Value.Stats.Attack,
                t.Value.Stats.Defense,
                t.Value.Stats.CarryCapacity,
                t.Value.Stats.Speed,
                t.Value.TrainedAt.ToString()
            )
        );

        return new GameConfigDto(buildings, troops);
    }
}
