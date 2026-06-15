using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Queries;

public record GetVillageQuery(Guid VillageId) : IRequest<Result<VillageDto>>;

public record VillageDto(
    Guid Id,
    string Name,
    ResourcesDto Resources,
    TroopsDto Troops,
    IReadOnlyList<BuildingDto> Buildings
);

public record ResourcesDto(int Wood, int Clay, int Iron, int Crop);
public record TroopsDto(int Swordsmen, int Archers, int Settlers);
public record BuildingDto(Guid Id, string Type, int Level);