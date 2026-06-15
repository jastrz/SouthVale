using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public record TroopEntry(TroopType TroopType, int Count);

public record CreateTrainOrderCommand(Guid VillageId, IReadOnlyList<TroopEntry> Orders)
    : IRequest<Result>;