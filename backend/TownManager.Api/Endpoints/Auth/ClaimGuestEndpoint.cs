using System.Security.Claims;
using MediatR;
using TownManager.Application.Auth.ClaimGuest;

namespace TownManager.Api.Endpoints.Auth;

public class ClaimGuestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/claim-guest", async (
            ClaimGuestRequest request,
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(
                new ClaimGuestCommand(userId, request.Email, request.NewPassword, request.Username), ct);

            if (!result.Succeeded)
                return result.ToHttpResponse();

            return Results.Ok(new { message = "Account claimed successfully" });
        })
        .WithName("ClaimGuest")
        .WithTags("Auth")
        .WithSummary("Claim a guest account")
        .WithDescription("Associates an email and sets a new password on a guest account. Requires authentication.")
        .RequireRateLimiting("Auth")
        .RequireAuthorization();
    }
}

public record ClaimGuestRequest(string Email, string NewPassword, string Username);
