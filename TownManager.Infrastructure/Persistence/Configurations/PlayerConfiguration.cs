using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TownManager.Infrastructure.Identity;

namespace TownManager.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(u => u.Player)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}