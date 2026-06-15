using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Commands;

public record CreateSettleOrderCommand(
    Guid VillageId,
    int TargetX,
    int TargetY) : IRequest<Result>;
