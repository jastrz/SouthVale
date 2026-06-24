using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using TownManager.Api.Configuration;
using TownManager.Api.ExceptionHandling;
using TownManager.Api.Services;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName));

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddAuthorization();

        services.AddOpenApi();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddSignalR();

        services.AddSingleton<IConnectedUserTracker, Hubs.ConnectedUserTracker>();
        services.AddSingleton<IGameNotificationService, GameNotificationService>();

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }
}
