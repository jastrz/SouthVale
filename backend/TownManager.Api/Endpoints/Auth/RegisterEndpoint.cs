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
                CancellationToken ct) =>
            {
                var result = await sender.Send(
                    new RegisterCommand(request.Email, request.Password, request.Username), ct);

                return result.ToHttpResponse();
            })
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with the provided email, password, and username, and returns a JWT access token.")
            .AllowAnonymous();
    }
}

public record RegisterRequest(string Email, string Password, string Username);