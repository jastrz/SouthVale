using System.Reflection;

namespace TownManager.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint))
                        && t is { IsAbstract: false, IsInterface: false });

        foreach (var type in endpointTypes)
            type.GetMethod("Map")!.Invoke(null, [app]);

        return app;
    }
}