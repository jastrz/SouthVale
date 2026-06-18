using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Entities;

namespace TownManager.Application.Villages.Commands;

public record CreateSettleOrderCommand(
    Guid VillageId,
    Coordinates Target) : IRequest<Result>;
