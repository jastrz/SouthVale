namespace TownManager.Api.Endpoints.Auth;

public class LogoutEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", async (HttpContext httpContext) =>
        {
            httpContext.Response.Cookies.Delete("refresh_token");
            return Results.Ok();
        })
        .WithName("Logout")
        .WithTags("Auth")
        .WithSummary("Log out")
        .WithDescription("Clears the refresh token cookie.")
        .RequireRateLimiting("Auth")
        .AllowAnonymous();
    }
}
