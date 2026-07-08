using System.Security.Claims;
using MediatR;
using TownManager.Application.Villages.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class GetCurrentUserVillagesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/me/villages", async (
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new GetCurrentUserVillagesQuery(userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetCurrentUserVillages")
        .WithTags("Gameplay")
        .WithSummary("Get current user villages")
        .WithDescription("Returns all villages owned by the authenticated player.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization();
    }
}
