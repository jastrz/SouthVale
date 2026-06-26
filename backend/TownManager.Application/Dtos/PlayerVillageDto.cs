using TownManager.Domain.Entities;

namespace TownManager.Application.Dtos;

public record PlayerVillageDto(
    Guid Id,
    Guid playerId,
    string Name,
    string PlayerName,
    Coordinates Coordinates,
    int Population
);
