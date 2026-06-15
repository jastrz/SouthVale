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
    ITokenService tokenService,
    IMediator mediator) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(string email, string password, string username, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var village = Village.CreateStarter(username);
        var player = Player.Create(username, village);
        
        db.Players.Add(player);
        db.Villages.Add(village);

        await db.SaveChangesAsync(ct);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PlayerId = player.Id
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.Select(e => e.Description));

        await transaction.CommitAsync(ct);

        var token = tokenService.GenerateToken(user.Id, user.Email, user.PlayerId);

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
        
        var token = tokenService.GenerateToken(user.Id, user.Email!, user.PlayerId);
        
        return Result<string>.Success(token);
    }
}