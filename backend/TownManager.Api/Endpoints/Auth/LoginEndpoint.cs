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
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new LoginCommand(request.Email, request.Password), ct);

            return result.Succeeded
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        })
        .WithName("Login")
        .WithTags("Auth")
        .WithSummary("Log in a user")
        .WithDescription("Authenticates a user with email and password and returns a JWT access token.")
        .AllowAnonymous();
    }
}

public record LoginRequest(string Email, string Password);