using System.Security.Claims;
using MediatR;
using TownManager.Application.Auth.Delete;

namespace TownManager.Api.Endpoints.Auth;

public class DeleteAccountEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/delete", async (
                DeleteRequest request,
                ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct
            ) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Results.Problem(statusCode: 401, title: "Unauthorized");

                var result = await sender.Send(new DeleteAccountCommand(userId, request.Password), ct);
                return result.ToHttpResponse();
            })
            .WithName("DeleteAccount")
            .WithTags("Auth")
            .WithSummary("Delete account")
            .WithDescription("Deletes account and associated player.")
            .RequireAuthorization()
            .RequireRateLimiting("Auth")
            .ProducesStandard<bool>(statusCodes: [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized]);
    }
}

public record DeleteRequest(string Password);