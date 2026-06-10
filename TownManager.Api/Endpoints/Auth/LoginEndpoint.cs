using TownManager.Application.Interfaces;

namespace TownManager.Api.Endpoints.Auth;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
                LoginRequest request,
                IAuthService authService,
                CancellationToken ct) =>
            {
                var result = await authService.LoginAsync(request.Email, request.Password, ct);
                return result.Succeeded
                    ? Results.Ok(new LoginResponse(result.Value!))
                    : Results.Unauthorized();
            })
            .WithName("Login")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken);