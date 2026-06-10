using Microsoft.AspNetCore.Identity;
using TownManager.Domain.Entities;

namespace TownManager.Infrastructure.Identity;

/// <summary>
/// Application user. Inherits the default Identity schema. Add custom
/// properties (e.g. DisplayName, CreatedAt) here as the domain grows.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
}
