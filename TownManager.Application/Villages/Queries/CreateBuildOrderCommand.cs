using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public record CreateBuildOrderCommand(Guid VillageId, BuildingType BuildingType) 
    : IRequest<Result>;