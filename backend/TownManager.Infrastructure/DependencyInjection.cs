using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TownManager.Application.Interfaces;
using TownManager.Application.Llm;
using TownManager.Domain.Config;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Jobs;
using TownManager.Infrastructure.Jobs.MovementResolvers;
using TownManager.Infrastructure.Llm;
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
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    },
                };
            });
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IVillageRepository, VillageRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        services.AddHttpClient<ILlmApiClient, LlmApiClient>(c => c.Timeout = TimeSpan.FromMinutes(5));

        services.AddScoped<IMovementResolver, AttackMovementResolver>();
        services.AddScoped<IMovementResolver, ReturnMovementResolver>();
        services.AddScoped<IMovementResolver, SettleMovementResolver>();
        services.AddScoped<IMovementResolver, TransportMovementResolver>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

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
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            options.WorkerCount = 5;
        });
        
        var llmConfig = configuration.GetSection(LlmPlayerConfig.SectionName).Get<LlmPlayerConfig>() ?? new();
        services.AddSingleton(llmConfig);

        var features = configuration.GetSection(FeatureFlags.SectionName).Get<FeatureFlags>() ?? new();
        services.AddSingleton(features);

        if (features.UseBarbarians)
            services.AddHostedService(sp => new BarbarianJobScheduler(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IRecurringJobManager>(),
                sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<ILogger<BarbarianJobScheduler>>()) { TickAtStart = true });

        if (features.UseLlmPlayers)
            services.AddHostedService(sp => new LlmPlayerJobScheduler(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IRecurringJobManager>(),
                sp.GetRequiredService<LlmPlayerConfig>(),
                sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<ILogger<LlmPlayerJobScheduler>>()) { TickAtStart = true });

        return services;
    }
}
