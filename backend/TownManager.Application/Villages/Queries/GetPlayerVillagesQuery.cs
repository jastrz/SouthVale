using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Villages.Queries;

public record GetPlayerVillagesQuery(string Username)
    : IRequest<Result<IReadOnlyList<PlayerVillageDto>>>;

