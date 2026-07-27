using System.Collections.Concurrent;
using MediatR;
using TownManager.Application.Auth.RegisterGuest;

namespace TownManager.Api.Endpoints.Auth;

public class RegisterGuestEndpoint : IEndpoint
{
    private static readonly ConcurrentDictionary<string, int> _guestIpCounts = new();
    private const int MaxGuestsPerIp = 3;
    private static readonly TimeSpan ResetTimeSpan = TimeSpan.FromHours(1);
    private static DateTime _lastCleanup = DateTime.UtcNow;

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register-guest", async (
            ISender sender,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (DateTime.UtcNow - _lastCleanup > ResetTimeSpan)
            {
                _guestIpCounts.Clear();
                _lastCleanup = DateTime.UtcNow;
            }

            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var count = _guestIpCounts.AddOrUpdate(ip, 1, (_, c) => c + 1);

            if (count > MaxGuestsPerIp)
                return Results.Problem(statusCode: 400, title: "Bad request", detail: "Too many guest accounts from this IP. You can reclaim your previous account.");

            var result = await sender.Send(new RegisterGuestCommand(), ct);

            if (!result.Succeeded)
                return result.ToHttpResponse();

            httpContext.Response.Cookies.Append("refresh_token", result.Value!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Secure = false,
                Path = "/",
            });

            return Results.Ok(new
            {
                accessToken = result.Value.AccessToken,
                username = result.Value.Username,
                password = result.Value.Password
            });
        })
        .WithName("RegisterGuest")
        .WithTags("Auth")
        .WithSummary("Register a guest user")
        .WithDescription("Creates a guest account with random credentials. Can be claimed later with email and password.")
        .RequireRateLimiting("Auth")
        .AllowAnonymous();
    }
}
