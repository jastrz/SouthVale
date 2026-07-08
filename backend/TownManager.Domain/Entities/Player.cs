using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities;

/// <summary>
/// A user account; owns one or more villages and has a currently active one.
/// </summary>
public class Player : Entity
{
    public required string Username { get; init; }
    public ICollection<Village> Villages { get; set; } = [];
    public string? UserId { get; init; }
    public bool IsBot { get; set; } = false;
    public BotPersonality BotPersonality { get; set; }

    public static Player Create(string username, string applicationUserId, Village starterVillage) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        Villages = [starterVillage],
        UserId = applicationUserId,
    };

    public static Player CreateBot(string username, string applicationUserId, Village starterVillage, BotPersonality botPersonality)
    {
        var player = Player.Create(username, applicationUserId, starterVillage);
        player.IsBot = true;
        player.BotPersonality = botPersonality;

        return player;
    }

}