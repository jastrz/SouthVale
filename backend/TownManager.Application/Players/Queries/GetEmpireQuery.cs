using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Players.Queries;

public record GetEmpireQuery(string UserId) : IRequest<Result<EmpireDto>>;