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
                return Results.Unauthorized();

            var result = await sender.Send(new GetCurrentUserMovementsQuery(userId), ct);
            return result.Succeeded
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        })
        .WithName("GetCurrentUserMovements")
        .WithTags("Gameplay")
        .WithSummary("Get current user movements")
        .WithDescription("Returns all active unit movements for the authenticated player.")
        .RequireAuthorization();
    }
}
