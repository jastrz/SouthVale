using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Queries;

public record GetCurrentUserVillagesQuery(string UserId)
    : IRequest<Result<IReadOnlyList<VillageDto>>>;
