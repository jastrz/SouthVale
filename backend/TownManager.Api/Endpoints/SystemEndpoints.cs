namespace TownManager.Api.Endpoints;

public class SystemEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new RootResponse("TownManager.Api", "ok")))
           .WithName("Root")
           .ProducesOk<RootResponse>(rateLimited: false);

        app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy")))
           .WithName("Health")
           .ProducesOk<HealthResponse>(rateLimited: false);
    }

    public record RootResponse(string Name, string Status);
    public record HealthResponse(string Status);
}
