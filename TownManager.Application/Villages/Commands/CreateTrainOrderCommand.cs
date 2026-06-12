using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public record TroopOrderEntry(TroopType TroopType, int Count);

public record CreateTrainOrderCommand(Guid VillageId, IReadOnlyList<TroopOrderEntry> Orders)
    : IRequest<Result>;