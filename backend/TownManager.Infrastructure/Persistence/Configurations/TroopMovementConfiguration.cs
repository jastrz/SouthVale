using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Infrastructure.Persistence.Configurations;

public class TroopMovementConfiguration : IEntityTypeConfiguration<TroopMovement>
{
    public void Configure(EntityTypeBuilder<TroopMovement> b)
    {
        b.OwnsOne(tm => tm.Troops, t => t.ToJson());
        b.OwnsOne(tm => tm.CarriedResources);
    }
}