using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Entities;

namespace TownManager.Application.Villages.Commands;

public record CreateAttackOrderCommand(
    Guid VillageId, 
    IReadOnlyList<TroopEntry> Troops, 
    Guid TargetVillageId
) : IRequest<Result>;
