using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Queries;

public record GetPlayerVillagesQuery(string Username)
    : IRequest<Result<IReadOnlyList<PlayerVillageDto>>>;

public record PlayerVillageDto(
    Guid Id,
    string Name,
    int MapX,
    int MapY,
    int Population
);
