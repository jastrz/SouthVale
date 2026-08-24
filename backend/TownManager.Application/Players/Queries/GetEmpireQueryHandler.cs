using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Players.Queries;

public class GetEmpireQueryHandler(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo)
    : IRequestHandler<GetEmpireQuery, Result<EmpireDto>>
{
    public async Task<Result<EmpireDto>> Handle(GetEmpireQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result<EmpireDto>.Failure(["Player not found."], statusCode: 404);

        var villages = await villageRepo.GetWithOrdersByPlayerAsync(player.Id, ct);
        var allBuildings = villages.SelectMany(v => v.Buildings).ToList();

        var maxTownHallLevel = allBuildings
            .Where(b => b.Type == BuildingType.TownHall)
            .Select(b => (int?)b.Level)
            .Max() ?? 0;

        return Result<EmpireDto>.Success(new EmpireDto(
            GameSettings.MaxBuildQueueSize + maxTownHallLevel,
            BuildingConfig.MaxEffect(allBuildings, BuildingType.Barracks, e => e.BarracksAttackMultiplier),
            BuildingConfig.MaxEffect(allBuildings, BuildingType.Stable, e => e.StableAttackMultiplier)
        ));
    }
}