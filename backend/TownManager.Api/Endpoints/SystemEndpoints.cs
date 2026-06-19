namespace TownManager.Api.Endpoints;

public class SystemEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new { name = "TownManager.Api", status = "ok" }))
           .WithName("Root");

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
           .WithName("Health");
    }
}
