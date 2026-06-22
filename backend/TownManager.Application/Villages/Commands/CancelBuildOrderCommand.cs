using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Commands;

public record CancelBuildOrderCommand(Guid OrderId) : IRequest<Result>;
