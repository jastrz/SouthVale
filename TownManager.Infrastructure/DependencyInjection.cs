using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Jobs;
using TownManager.Infrastructure.Persistence;
using TownManager.Infrastructure.Persistence.Repositories;

namespace TownManager.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Wires up the EF Core context (PostgreSQL) and ASP.NET Core Identity
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
                               ?? throw new InvalidOperationException(
                                   "ConnectionStrings:Postgres is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IVillageRepository, VillageRepository>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        
        // Hangfire Configuration
        
        var storageOptions = new PostgreSqlStorageOptions
        {
            PrepareSchemaIfNecessary = true,
            StartupConnectionMaxRetries = 0,
            AllowDegradedModeWithoutStorage = false, 
        };
        
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => 
                c.UseNpgsqlConnection(configuration.GetConnectionString("Postgres")),
                storageOptions));

        services.AddHangfireServer(options =>
        {
            options.HeartbeatInterval = TimeSpan.FromSeconds(15);
            options.WorkerCount = 5;
        });

        return services;
    }
}
