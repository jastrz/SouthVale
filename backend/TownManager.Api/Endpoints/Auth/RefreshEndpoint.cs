using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Identity;

namespace TownManager.Api.Endpoints.Auth;

public class RefreshEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async (
            HttpContext httpContext,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            ILogger<RefreshEndpoint> logger,
            CancellationToken ct) =>
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("Refresh failed: no refresh_token cookie");
                return Results.Problem(statusCode: 401, title: "No refresh token");
            }

            var principal = tokenService.ValidateToken(refreshToken);
            if (principal is null)
            {
                httpContext.Response.Cookies.Delete("refresh_token");
                logger.LogWarning("Refresh failed: invalid or expired refresh token");
                return Results.Problem(statusCode: 401, title: "Invalid or expired refresh token");
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (userId is null || email is null)
            {
                logger.LogWarning("Refresh failed: invalid token claims (userId={UserId}, email={Email})", userId, email);
                return Results.Problem(statusCode: 401, title: "Invalid token claims");
            }

            var user = await userManager.FindByIdAsync(userId);
            var roles = user is not null ? await userManager.GetRolesAsync(user) : [];

            var newAccessToken = tokenService.GenerateAccessToken(userId, email, roles);
            var newRefreshToken = tokenService.GenerateRefreshToken(userId, email);

            httpContext.Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Secure = false,
                Path = "/",
            });

            logger.LogInformation("Token refreshed: {Email}", email);

            return Results.Ok(new RefreshResponse(newAccessToken));
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .WithSummary("Refresh access token")
        .WithDescription("Validates the refresh_token cookie and issues a new access token + rotates the refresh token.")
        .RequireRateLimiting("Auth")
        .AllowAnonymous()
        .ProducesStandard<RefreshResponse>(statusCodes: [StatusCodes.Status401Unauthorized]);
    }

    public record RefreshResponse(string AccessToken);
}
