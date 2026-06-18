using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Villages.Queries;

public record GetCurrentUserVillagesQuery(string UserId)
    : IRequest<Result<IReadOnlyList<VillageDto>>>;
