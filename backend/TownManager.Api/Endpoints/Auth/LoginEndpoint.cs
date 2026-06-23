using MediatR;
using TownManager.Application.Auth.Login;

namespace TownManager.Api.Endpoints.Auth;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
            LoginRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new LoginCommand(request.Email, request.Password), ct);

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

            return Results.Ok(new { accessToken = result.Value.AccessToken, username = result.Value.Username });
        })
        .WithName("Login")
        .WithTags("Auth")
        .WithSummary("Log in a user")
        .WithDescription("Authenticates a user with email and password and returns a JWT access token.")
        .AllowAnonymous();
    }
}

public record LoginRequest(string Email, string Password);
