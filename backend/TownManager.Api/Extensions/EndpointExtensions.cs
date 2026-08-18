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

public static class EndpointConventions
{
    private static readonly int[] RateLimitProblemStatusCodes = [429];

    private static readonly int[] InternalErrorProblemStatusCodes = [500];

    public static RouteHandlerBuilder ProducesStandard<T>(
        this RouteHandlerBuilder builder, bool rateLimited = true, params int[] statusCodes) =>
        builder.Produces<T>(StatusCodes.Status200OK)
            .ProducesStandardProblems(rateLimited, statusCodes);

    public static RouteHandlerBuilder ProducesStandard(
        this RouteHandlerBuilder builder, bool rateLimited = true, params int[] statusCodes) =>
        builder.Produces(StatusCodes.Status200OK)
            .ProducesStandardProblems(rateLimited, statusCodes);

    public static RouteHandlerBuilder ProducesOk<T>(
        this RouteHandlerBuilder builder, bool rateLimited = true) =>
        builder.Produces<T>(StatusCodes.Status200OK)
            .ProducesOkProblems(rateLimited);

    private static RouteHandlerBuilder ProducesOkProblems(
        this RouteHandlerBuilder builder, bool rateLimited)
    {
        if (rateLimited)
            foreach (var code in RateLimitProblemStatusCodes)
                builder.ProducesProblem(code);
        return builder;
    }

    private static RouteHandlerBuilder ProducesStandardProblems(
        this RouteHandlerBuilder builder, bool rateLimited, params int[] statusCodes)
    {
        foreach (var code in statusCodes)
            builder.ProducesProblem(code);
        if (rateLimited)
            foreach (var code in RateLimitProblemStatusCodes)
                builder.ProducesProblem(code);
        foreach (var code in InternalErrorProblemStatusCodes)
            builder.ProducesProblem(code);
        return builder;
    }
}