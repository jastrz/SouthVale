using System.Security.Claims;
using MediatR;
using TownManager.Application.Reports.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class MarkReportsReadEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/me/reports/read", async (
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new MarkReportsReadCommand(userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("MarkReportsRead")
        .WithTags("Gameplay")
        .WithSummary("Mark all reports as read")
        .WithDescription("Marks all unread reports for the current player as read.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .ProducesStandard(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status404NotFound]);
    }
}
