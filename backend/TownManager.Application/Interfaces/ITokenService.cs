using System.Security.Claims;

namespace TownManager.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string email);
    string GenerateRefreshToken(string userId, string email);
    ClaimsPrincipal? ValidateToken(string token);
}