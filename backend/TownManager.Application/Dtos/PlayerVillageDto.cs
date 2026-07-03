using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Application.Dtos;

public record PlayerVillageDto(
    Guid Id,
    Guid PlayerId,
    string Name,
    string PlayerName,
    Coordinates Coordinates,
    int Population,
    VillageType VillageType
);
