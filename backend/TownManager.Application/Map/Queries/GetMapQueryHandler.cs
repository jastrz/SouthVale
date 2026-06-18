using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;

namespace TownManager.Application.Map.Queries;

public class GetMapQueryHandler(IVillageRepository villageRepo)
    : IRequestHandler<GetMapQuery, Result<IReadOnlyList<PlayerVillageDto>>>
{
    public async Task<Result<IReadOnlyList<PlayerVillageDto>>> Handle(GetMapQuery request, CancellationToken ct)
    {
        var villages = await villageRepo.GetForMapWithinRadius(request.cords, request.radius, ct);

        var dtos = villages
            .Select(v => new PlayerVillageDto(v.Id, v.PlayerId, v.Name, v.Coordinates, v.Troops.TotalCount))
            .ToList();

        return Result<IReadOnlyList<PlayerVillageDto>>.Success(dtos);
    }
}