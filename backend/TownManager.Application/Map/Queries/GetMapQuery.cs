using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Domain.Entities;

namespace TownManager.Application.Map.Queries;

public record GetMapQuery(Coordinates cords, int radius)
    : IRequest<Result<IReadOnlyList<PlayerVillageDto>>>;