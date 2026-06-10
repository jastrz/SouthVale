using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Identity;

namespace TownManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Player> Players  => Set<Player>();
    public DbSet<Village> Villages => Set<Village>();
    public DbSet<BuildOrder> BuildOrders => Set<BuildOrder>();
    public DbSet<TrainOrder> TrainOrders => Set<TrainOrder>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}