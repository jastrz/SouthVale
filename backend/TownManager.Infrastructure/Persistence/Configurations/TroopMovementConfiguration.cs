using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Persistence.Configurations;

public class TroopMovementConfiguration : IEntityTypeConfiguration<TroopMovement>
{
    public void Configure(EntityTypeBuilder<TroopMovement> b)
    {
        b.OwnsOne(tm => tm.Troops, tb =>
        {
            tb.Property(p => p.Counts)
              .HasColumnType("jsonb")
              .HasConversion(
                  v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                  v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<TroopType, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<TroopType, int>());
        });
        b.OwnsOne(tm => tm.CarriedResources);

        b.ComplexProperty(tm => tm.TargetCoordinates, cb =>
        {
            cb.Property(c => c.X).HasColumnName("TargetMapX");
            cb.Property(c => c.Y).HasColumnName("TargetMapY");
        });
    }
}