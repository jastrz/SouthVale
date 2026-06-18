using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public record GetCurrentUserMovementsQuery(string UserId)
    : IRequest<Result<IReadOnlyList<MovementDto>>>;

