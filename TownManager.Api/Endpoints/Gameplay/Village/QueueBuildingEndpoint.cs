using MediatR;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Enums;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class QueueBuildingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/build", async (
            Guid villageId,
            CreateBuildOrderRequest request,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var result = await sender.Send(new CreateBuildOrderCommand(villageId, request.BuildingType), ct);
            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        })
        .WithName("CreateBuildOrder")
        .WithTags("Gameplay")
        .AllowAnonymous();
    }

    public record CreateBuildOrderRequest(BuildingType BuildingType);
}