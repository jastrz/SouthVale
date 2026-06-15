using MediatR;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Enums;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class EnqueueBuildingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/build", async (
            Guid villageId,
            EnqueueBuildingRequest request,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var result = await sender.Send(new CreateBuildOrderCommand(villageId, request.BuildingType), ct);
            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        })
        .WithName("QueueBuilding")
        .WithTags("Gameplay")
        .WithSummary("Queue a building construction")
        .WithDescription("Enqueues a construction order for the specified building type in the village's build queue.")
        .AllowAnonymous();
    }

    public record EnqueueBuildingRequest(BuildingType BuildingType);
}