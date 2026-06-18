using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Entities;

namespace TownManager.Application.Villages.Queries;

public record GetPlayerVillagesQuery(string Username)
    : IRequest<Result<IReadOnlyList<PlayerVillageDto>>>;

public record PlayerVillageDto(
    Guid Id,
    string Name,
    Coordinates Coordinates,
    int Population
);
