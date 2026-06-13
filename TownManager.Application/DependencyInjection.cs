using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TownManager.Application.Common;

namespace TownManager.Application;

public sealed class ApplicationAssemblyMarker;

public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR handlers and FluentValidation validators from this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
