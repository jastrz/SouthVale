using System.Security.Claims;
using MediatR;
using TownManager.Application.Reports.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class GetReportsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/me/reports", async (
            ISender sender,
            ClaimsPrincipal user,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(
                new GetReportsQuery(userId, Math.Max(1, page ?? 1), Math.Clamp(pageSize ?? 10, 1, 100)), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetReports")
        .WithTags("Gameplay")
        .WithSummary("Get reports for current player")
        .WithDescription("Returns movement and combat reports for the authenticated player, newest first.")
        .RequireAuthorization();
    }
}
