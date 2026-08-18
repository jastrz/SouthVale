using System.Security.Claims;
using MediatR;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class CancelBuildOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/build/{orderId:guid}/cancel", async (
            Guid orderId,
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new CancelBuildOrderCommand(orderId, userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("CancelBuildOrder")
        .WithTags("Gameplay")
        .WithSummary("Cancel a build order")
        .WithDescription("Cancels a queued build order and refunds the resources.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .ProducesStandard(statusCodes: [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound]);
    }
}
