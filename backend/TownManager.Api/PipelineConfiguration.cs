using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TownManager.Application.Map.Services;
using TownManager.Domain.Config;
using TownManager.Api.Hubs;
using TownManager.Infrastructure.Data;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Api;

public static class PipelineConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            db.Database.Migrate();
            
            if (bool.TryParse(app.Configuration["Features:UseBarbarians"], out var useBarb) && useBarb)
            {
                var barbarianSeeder = new BarbarianSeeder(db);
                barbarianSeeder.SeedAsync().GetAwaiter().GetResult();
                Log.Information("Barbarian player ready.");
            }

            if (bool.TryParse(app.Configuration["Features:UseLlmPlayers"], out var useLlm) && useLlm)
            {
                var llmLogger = scope.ServiceProvider.GetRequiredService<ILogger<LlmPlayerSeeder>>();
                var mapService = scope.ServiceProvider.GetRequiredService<IMapService>();
                var llmSeeder = new LlmPlayerSeeder(userManager, db, mapService, llmLogger);
                llmSeeder.SeedAsync().GetAwaiter().GetResult();
                Log.Information("LLM players ready.");
            }
            
            if (!string.IsNullOrWhiteSpace(app.Configuration["Admin:Password"]))
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var adminLogger = scope.ServiceProvider.GetRequiredService<ILogger<AdminSeeder>>();
                var adminSeeder = new AdminSeeder(userManager, roleManager, app.Configuration, adminLogger);
                adminSeeder.SeedAsync().GetAwaiter().GetResult();
                Log.Information("Admin ready.");
            }
            else
            {
                Log.Information("Admin:Password not set — skipping admin seed");
            }
        }

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (userManager.Users.Any() == false)
            {
                var seeder = new TestDataSeeder(userManager, db);
                seeder.SeedAsync().GetAwaiter().GetResult();
                Log.Information("Db seeded.");
            }
        }

        app.MapOpenApi();
        app.MapScalarApiReference(opt =>
        {
            opt.BaseServerUrl = "/southvale/api";
        });

        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<GameHub>("/hubs/game");

        return app;
    }
}
