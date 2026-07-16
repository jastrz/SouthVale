using System.Security.Claims;

namespace TownManager.Api.Endpoints.Auth;

public class LogoutEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", async (
            HttpContext httpContext,
            ClaimsPrincipal user,
            ILogger<LogoutEndpoint> logger) =>
        {
            httpContext.Response.Cookies.Delete("refresh_token");

            var email = user.FindFirstValue(ClaimTypes.Email) ?? "unknown";
            logger.LogInformation("User logged out: {Email}", email);

            return Results.Ok();
        })
        .WithName("Logout")
        .WithTags("Auth")
        .WithSummary("Log out")
        .WithDescription("Clears the refresh token cookie.")
        .RequireRateLimiting("Auth")
        .RequireAuthorization();
    }
}
