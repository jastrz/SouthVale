using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public record GetVillageQuery(Guid VillageId) : IRequest<Result<VillageDto>>;





