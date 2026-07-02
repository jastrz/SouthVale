using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TownManager.Application.Common;
using TownManager.Application.Villages.Services;

namespace TownManager.Application;

public sealed class ApplicationAssemblyMarker;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IBarbarianTickService, BarbarianTickService>();

        return services;
    }
}
