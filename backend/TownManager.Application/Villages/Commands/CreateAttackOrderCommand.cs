using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Commands;

public record CreateAttackOrderCommand(
    Guid VillageId, 
    IReadOnlyList<TroopEntry> Troops, 
    Guid TargetVillageId
) : IRequest<Result>;
