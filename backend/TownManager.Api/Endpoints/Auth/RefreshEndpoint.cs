using System.Security.Claims;
using TownManager.Application.Interfaces;

namespace TownManager.Api.Endpoints.Auth;

public class RefreshEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async (
            HttpContext httpContext,
            ITokenService tokenService,
            CancellationToken ct) =>
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Results.Problem(statusCode: 401, title: "No refresh token");

            var principal = tokenService.ValidateToken(refreshToken);
            if (principal is null)
            {
                httpContext.Response.Cookies.Delete("refresh_token");
                return Results.Problem(statusCode: 401, title: "Invalid or expired refresh token");
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (userId is null || email is null)
                return Results.Problem(statusCode: 401, title: "Invalid token claims");

            var newAccessToken = tokenService.GenerateAccessToken(userId, email);
            var newRefreshToken = tokenService.GenerateRefreshToken(userId, email);

            httpContext.Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Secure = false,
                Path = "/",
            });

            return Results.Ok(new { accessToken = newAccessToken });
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .WithSummary("Refresh access token")
        .WithDescription("Validates the refresh_token cookie and issues a new access token + rotates the refresh token.")
        .AllowAnonymous();
    }
}
