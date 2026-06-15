using MediatR;
using Microsoft.AspNetCore.Identity;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    ITokenService tokenService
    ) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(string email, string password, string username, CancellationToken ct)
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
            return Result<string>.Failure(result.Errors.Select(e => e.Description));

        var village = Village.CreateStarter($"{username}'s village", (0, 0));
        var player = Player.Create(username, user.Id, village);

        db.Players.Add(player);
        db.Villages.Add(village);
        
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    
        var token = tokenService.GenerateToken(user.Id, user.Email);
    
        return Result<string>.Success(token);
    }

    public async Task<Result<string>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<string>.Failure(["Invalid email or password"]);

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
            return Result<string>.Failure(["Invalid email or password"]);
        
        var token = tokenService.GenerateToken(user.Id, user.Email!);
        
        return Result<string>.Success(token);
    }
}