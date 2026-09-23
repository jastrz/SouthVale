using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.World.Queries;

public record GetWorldStatusQuery : IRequest<Result<WorldStatusDto>>;
