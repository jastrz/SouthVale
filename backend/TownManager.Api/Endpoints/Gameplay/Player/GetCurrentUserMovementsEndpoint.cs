using System.Security.Claims;
using MediatR;
using TownManager.Application.Villages.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class GetCurrentUserMovementsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/me/movements", async (
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new GetCurrentUserMovementsQuery(userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetCurrentUserMovements")
        .WithTags("Gameplay")
        .WithSummary("Get current user movements")
        .WithDescription("Returns all active unit movements for the authenticated player.")
        .RequireAuthorization();
    }
}
