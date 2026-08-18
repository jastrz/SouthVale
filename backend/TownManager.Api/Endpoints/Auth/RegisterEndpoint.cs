using MediatR;
using TownManager.Application.Auth.Register;

namespace TownManager.Api.Endpoints.Auth;

public class RegisterEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            RegisterRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new RegisterCommand(request.Email, request.Password, request.Username), ct);

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

            return Results.Ok(new AuthResponse(result.Value.AccessToken, result.Value.Username));
        })
        .WithName("Register")
        .WithTags("Auth")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account with the provided email, password, and username, and returns a JWT access token.")
        .RequireRateLimiting("Auth")
        .AllowAnonymous()
        .ProducesStandard<AuthResponse>(statusCodes: [StatusCodes.Status400BadRequest]);
    }
}

public record RegisterRequest(string Email, string Password, string Username);