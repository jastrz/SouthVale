namespace TownManager.Api.Configuration;

/// <summary>
/// CORS settings, bound from the <c>Cors</c> configuration section.
/// <see cref="AllowedOrigins"/> drives the default CORS policy in
/// <c>Program.cs</c>.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
}
