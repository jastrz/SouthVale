using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Identity;

namespace TownManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options, IMediator mediator) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Player> Players  => Set<Player>();
    public DbSet<Village> Villages => Set<Village>();
    public DbSet<BuildOrder> BuildOrders => Set<BuildOrder>();
    public DbSet<TrainOrder> TrainOrders => Set<TrainOrder>();
    public DbSet<TroopMovement> TroopMovements => Set<TroopMovement>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entries = ChangeTracker.Entries<Entity>();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.Events.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entitiesWithEvents.SelectMany(e => e.Events).ToList();

        var saved = await base.SaveChangesAsync(ct);

        // Publish events after commit
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());
        foreach (var _event in events)
            await mediator.Publish(_event, ct);

        return saved;
    }
}