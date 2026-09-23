using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TownManager.Domain.Entities;

namespace TownManager.Infrastructure.Persistence.Configurations;

public class WorldIterationConfiguration : IEntityTypeConfiguration<WorldIteration>
{
    public void Configure(EntityTypeBuilder<WorldIteration> builder)
    {
        builder.ToTable("WorldIterations");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Number).IsRequired();
        builder.Property(i => i.StartedAt).IsRequired();

        builder.OwnsMany(i => i.Winners, w => w.ToJson());
    }
}
