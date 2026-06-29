using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Villages.Queries;

public record GetCurrentUserVillagesStatusQuery(string UserId)
    : IRequest<Result<IReadOnlyList<VillageStatusDto>>>;
