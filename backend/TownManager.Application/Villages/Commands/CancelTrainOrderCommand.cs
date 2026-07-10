using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Commands;

public record CancelTrainOrderCommand(Guid OrderId, string UserId) : IRequest<Result>;
