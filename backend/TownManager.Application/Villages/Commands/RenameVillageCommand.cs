using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Villages.Commands;

public record RenameVillageCommand(Guid VillageId, string NewName)
    : IRequest<Result>;
