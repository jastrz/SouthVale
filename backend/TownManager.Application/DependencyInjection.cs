using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Application.Map.Services;
using TownManager.Application.Players.Services;
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
        services.AddScoped<IVillageTickService, VillageTickService>();
        services.AddScoped<ILlmPlayerService, LlmPlayerService>();
        services.AddScoped<IMapService, MapService>();

        return services;
    }
}
