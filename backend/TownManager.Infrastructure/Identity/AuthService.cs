using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Application.Map.Services;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    ITokenService tokenService,
    IMapService mapService,
    ILogger<AuthService> logger
    ) : IAuthService
{
    public async Task<Result<LoginResult>> RegisterAsync(string email, string password, string username, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result<LoginResult>.Failure(result.Errors.Select(e => e.Description));

        var coords = (await mapService.GetFreeTilesAsync(1, ct)).FirstOrDefault() ?? new Coordinates(0, 0);
        var village = Village.CreateStarter($"{username}'s village", coords);
        var player = Player.Create(username, user.Id, village);

        db.Players.Add(player);
        db.Villages.Add(village);

        await db.SaveChangesAsync(ct);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id, user.Email!);

        logger.LogInformation("User registered: {Username} ({Email}), village: {Village}", username, email, village.Name);

        return Result<LoginResult>.Success(new LoginResult(accessToken, refreshToken, username));
    }

    public async Task<Result<LoginResult>> RegisterGuestAsync(CancellationToken ct)
    {
        var username = $"Guest_{Random.Shared.Next(100000, 999999)}";
        var password = $"Guest{Guid.NewGuid().ToString("N")[..8]}!A";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = username,
            Email = null,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return Result<LoginResult>.Failure(createResult.Errors.Select(e => e.Description));

        var coords = (await mapService.GetFreeTilesAsync(1, ct)).FirstOrDefault() ?? new Coordinates(0, 0);
        var village = Village.CreateStarter($"{username}'s village", coords);
        var player = Player.Create(username, user.Id, village);

        db.Players.Add(player);
        db.Villages.Add(village);

        await db.SaveChangesAsync(ct);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, string.Empty, roles);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id, string.Empty);

        logger.LogInformation("Guest registered: {Username}, village: {Village}", username, village.Name);

        return Result<LoginResult>.Success(new LoginResult(accessToken, refreshToken, username) { Password = password });
    }

    public async Task<Result> ClaimGuestAsync(string userId, string email, string newPassword, string username, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(["User not found"]);

        if (!string.IsNullOrEmpty(user.Email))
            return Result.Failure(["Account already claimed"]);

        var emailOwner = await userManager.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != user.Id)
            return Result.Failure(["Email already in use"]);

        var player = await db.Players.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (player is not null)
        {
            db.Entry(player).Property("Username").CurrentValue = username;
        }

        user.Email = email;
        user.UserName = email;
        user.EmailConfirmed = true;

        var setResult = await userManager.UpdateAsync(user);
        if (!setResult.Succeeded)
            return Result.Failure(setResult.Errors.Select(e => e.Description));

        var passwordErrors = new List<string>();
        foreach (var validator in userManager.PasswordValidators)
        {
            var vr = await validator.ValidateAsync(userManager, user, newPassword);
            if (!vr.Succeeded)
                passwordErrors.AddRange(vr.Errors.Select(e => e.Description));
        }
        if (passwordErrors.Count != 0)
            return Result.Failure(passwordErrors);

        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, newPassword);
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Failure(updateResult.Errors.Select(e => e.Description));

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Guest claimed: {UserId} -> {Email}, username: {Username}", userId, email, username);

        return Result.Success();
    }

    public async Task<Result<LoginResult>> LoginAsync(string emailOrUsername, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(emailOrUsername)
                   ?? await userManager.FindByNameAsync(emailOrUsername);
        if (user is null)
            return Result<LoginResult>.Failure(["Invalid email/username or password"]);

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
            return Result<LoginResult>.Failure(["Invalid email/username or password"]);

        var player = await db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        var username = player?.Username ?? user.UserName ?? "unknown";

        logger.LogInformation("User logged in: {Username} ({Email})", username, user.Email ?? "(guest)");

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, roles);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id, user.Email ?? string.Empty);

        return Result<LoginResult>.Success(new LoginResult(accessToken, refreshToken, username));
    }

    public async Task<Result<bool>> DeleteAsync(string userId, string password, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<bool>.Failure(["User not found"]);

        if (!await userManager.CheckPasswordAsync(user, password))
            return Result<bool>.Failure(["Wrong password"]);

        var player = await db.Players.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (player is not null)
        {
            db.Players.Remove(player);
            await db.SaveChangesAsync(ct);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return Result<bool>.Failure(result.Errors.Select(e => e.Description));

        await db.SaveChangesAsync(ct);

        logger.LogInformation("User deleted: {Email}", user.Email);

        return Result<bool>.Success(true);
    }
}
