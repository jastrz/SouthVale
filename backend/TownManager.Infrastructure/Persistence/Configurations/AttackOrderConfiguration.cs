using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Infrastructure.Persistence.Configurations;

// public class AttackOrderConfiguration : IEntityTypeConfiguration<TroopMovement>
// {
//     public void Configure(EntityTypeBuilder<TroopMovement> b)
//     {
//         b.OwnsOne(a => a.Troops, t => t.ToJson());
//     }
// } 