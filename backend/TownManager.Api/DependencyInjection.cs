using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
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
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddAuthorization();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<ProductionServerUrlTransformer>();
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddSignalR();

        services.AddSingleton<IConnectedUserTracker, Hubs.ConnectedUserTracker>();
        services.AddSingleton<IGameNotificationService, GameNotificationService>();

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Auth", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("Gameplay", opt =>
            {
                opt.PermitLimit = 1000;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        });

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

internal sealed class ProductionServerUrlTransformer(
    IWebHostEnvironment environment,
    IConfiguration configuration)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (environment.IsProduction())
        {
            document.Servers =
            [
                new OpenApiServer { Url = configuration["Scalar:ServerUrl"] ?? "/southvale/api" }
            ];
        }

        return Task.CompletedTask;
    }
}
