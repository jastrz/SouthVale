using MediatR;
using TownManager.Api.Endpoints.Filters;
using TownManager.Application.Dtos;
using TownManager.Application.Villages.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class GetVillageEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/village/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var result = await sender.Send(new GetVillageQuery(id), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetVillage")
        .WithTags("Gameplay")
        .WithSummary("Get a village by id")
        .WithDescription("Returns the full state of a village, including resources, buildings, troops, and active build and train orders.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .AddEndpointFilter<VillageOwnershipFilter>()
        .ProducesStandard<VillageDto>(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound]);
    }
}