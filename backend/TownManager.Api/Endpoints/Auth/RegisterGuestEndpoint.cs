using MediatR;
using TownManager.Application.Auth.RegisterGuest;

namespace TownManager.Api.Endpoints.Auth;

public class RegisterGuestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register-guest", async (
            ISender sender,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
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
