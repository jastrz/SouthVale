using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public record CreateBuildOrderCommand(Guid VillageId, BuildingType BuildingType, bool IsBot = false) 
    : IRequest<Result>;