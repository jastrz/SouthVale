using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace TownManager.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR handlers and FluentValidation validators from this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
