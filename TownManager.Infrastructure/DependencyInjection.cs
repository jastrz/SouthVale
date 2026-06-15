using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TownManager.Application.Interfaces;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Jobs;
using TownManager.Infrastructure.Jobs.MovementResolvers;
using TownManager.Infrastructure.Persistence;
using TownManager.Infrastructure.Persistence.Repositories;

namespace TownManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
                               ?? throw new InvalidOperationException(
                                   "ConnectionStrings:Postgres is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                                               ?? throw new InvalidOperationException("Jwt:Key is not configured."))),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IVillageRepository, VillageRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        
        services.AddScoped<IMovementResolver, AttackMovementResolver>();
        services.AddScoped<IMovementResolver, ReturnMovementResolver>();

        
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
