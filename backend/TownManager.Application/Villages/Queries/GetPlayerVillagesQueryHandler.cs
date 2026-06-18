using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Villages.Queries;

public class GetPlayerVillagesQueryHandler(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo)
    : IRequestHandler<GetPlayerVillagesQuery, Result<IReadOnlyList<PlayerVillageDto>>>
{
    public async Task<Result<IReadOnlyList<PlayerVillageDto>>> Handle(
        GetPlayerVillagesQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUsernameAsync(q.Username, ct);
        if (player is null)
            return Result<IReadOnlyList<PlayerVillageDto>>.Failure(["Player not found."]);

        var villages = await villageRepo.GetSummariesByPlayerAsync(player.Id, ct);

        var dtos = villages
            .Select(v => new PlayerVillageDto(v.Id, v.PlayerId, v.Name, v.Coordinates, v.Troops.TotalCount))
            .ToList();

        return Result<IReadOnlyList<PlayerVillageDto>>.Success(dtos);
    }
}
