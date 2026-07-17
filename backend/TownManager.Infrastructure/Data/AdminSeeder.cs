using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Data;

public class AdminSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<AdminSeeder> logger)
{
    public async Task SeedAsync()
    {
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Admin:Password not configured — skipping admin seed");
            return;
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var admin = await userManager.FindByEmailAsync("admin@townmanager.local");
        if (admin is not null) return;

        admin = new ApplicationUser
        {
            UserName = "admin@townmanager.local",
            Email = "admin@townmanager.local",
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
