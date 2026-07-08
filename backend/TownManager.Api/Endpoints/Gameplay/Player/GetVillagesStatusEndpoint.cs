using System.Security.Claims;
using MediatR;
using TownManager.Application.Villages.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class GetVillagesStatusEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/me/villages/status", async (
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new GetCurrentUserVillagesStatusQuery(userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetVillagesStatus")
        .WithTags("Gameplay")
        .WithSummary("Get active order counts per village")
        .WithDescription("Returns build and train order counts for all villages owned by the authenticated player.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization();
    }
}
