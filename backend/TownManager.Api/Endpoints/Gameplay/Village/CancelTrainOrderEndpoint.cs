using System.Security.Claims;
using MediatR;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class CancelTrainOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/train/{orderId:guid}/cancel", async (
            Guid orderId,
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new CancelTrainOrderCommand(orderId, userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("CancelTrainOrder")
        .WithTags("Gameplay")
        .WithSummary("Cancel a training order")
        .WithDescription("Cancels a queued training order and refunds the uncompleted portion.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization();
    }
}
