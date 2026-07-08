using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    ITokenService tokenService,
    ILogger<AuthService> logger
    ) : IAuthService
{
    public async Task<Result<LoginResult>> RegisterAsync(string email, string password, string username, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result<LoginResult>.Failure(result.Errors.Select(e => e.Description));

        var village = Village.CreateStarter($"{username}'s village", new Coordinates(0, 0));
        var player = Player.Create(username, user.Id, village);

        db.Players.Add(player);
        db.Villages.Add(village);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id, user.Email!);

        logger.LogInformation("User registered: {Username} ({Email}), village: {Village}", username, email, village.Name);

        return Result<LoginResult>.Success(new LoginResult(accessToken, refreshToken, username));
    }

    public async Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<LoginResult>.Failure(["Invalid email or password"]);

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
            return Result<LoginResult>.Failure(["Invalid email or password"]);

        var player = await db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        var username = player?.Username ?? "Unknown";

        logger.LogInformation("User logged in: {Username} ({Email})", username, email);

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id, user.Email!);

        return Result<LoginResult>.Success(new LoginResult(accessToken, refreshToken, username));
    }
}
