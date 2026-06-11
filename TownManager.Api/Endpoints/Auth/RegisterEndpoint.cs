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

                return result.Succeeded
                    ? Results.Ok(result.Value)
                    : Results.Problem(
                        title: "Registration failed",
                        detail: string.Join(", ", result.Errors),
                        statusCode: 400);
            })
            .WithName("Register")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}

public record RegisterRequest(string Email, string Password, string Username);