using System.Security.Claims;
using MediatR;
using TownManager.Application.Reports.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class MarkReportReadEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/me/reports/{id:guid}/read", async (
            Guid id,
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new MarkReportReadCommand(userId, id), ct);
            return result.ToHttpResponse();
        })
        .WithName("MarkReportRead")
        .WithTags("Gameplay")
        .WithSummary("Mark a single report as read")
        .WithDescription("Marks a specific report as read for the current player.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization();
    }
}
