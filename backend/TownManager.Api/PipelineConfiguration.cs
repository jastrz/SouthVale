using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
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

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            db.Database.EnsureCreated();

            if (userManager.Users.Any() == false)
            {
                var seeder = new TestDataSeeder(userManager, db);
                seeder.SeedAsync().GetAwaiter().GetResult();
                Log.Information("Db seeded.");
            }
        }

        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
