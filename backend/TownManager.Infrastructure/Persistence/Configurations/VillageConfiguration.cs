using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Persistence.Configurations;

public class VillageConfiguration : IEntityTypeConfiguration<Village>
{
    public void Configure(EntityTypeBuilder<Village> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.UpdatedAt).IsConcurrencyToken();

        b.HasOne(v => v.Player)
         .WithMany(p => p.Villages)
         .HasForeignKey(v => v.PlayerId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(v => v.Buildings)
         .WithOne()
         .HasForeignKey(x => x.VillageId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(v => v.BuildOrders)
         .WithOne(x => x.Village)
         .HasForeignKey(x => x.VillageId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(v => v.TrainOrders)
         .WithOne(x => x.Village)
         .HasForeignKey(x => x.VillageId)
         .OnDelete(DeleteBehavior.Cascade);
        
        b.HasMany(v => v.TroopMovements)
         .WithOne(x => x.Village)
         .HasForeignKey(v => v.VillageId)
         .OnDelete(DeleteBehavior.Cascade);
        
        b.OwnsOne(v => v.Resources, rb =>
        {
          rb.Property(p => p.Wood).HasColumnName("Wood");
          rb.Property(p => p.Clay).HasColumnName("Clay");
          rb.Property(p => p.Iron).HasColumnName("Iron");
          rb.Property(p => p.Beer).HasColumnName("Beer");
        });

        b.OwnsOne(v => v.Troops, tb =>
        {
            tb.Property(p => p.Counts)
              .HasColumnType("jsonb")
              .HasConversion(
                  v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                  v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<TroopType, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<TroopType, int>());
        });

        b.ComplexProperty(v => v.Coordinates, cb =>
        {
            cb.Property(c => c.X).HasColumnName("MapX");
            cb.Property(c => c.Y).HasColumnName("MapY");
        });
    }
}