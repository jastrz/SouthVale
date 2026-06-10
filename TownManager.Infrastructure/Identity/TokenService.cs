using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Identity;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(string userId, string email, Guid playerId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("playerId", playerId.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Authentication:Schemes:Bearer:SigningKey"]!));

        var token = new JwtSecurityToken(
            issuer: configuration["Authentication:Schemes:Bearer:ValidIssuer"],
            audience: configuration["Authentication:Schemes:Bearer:ValidAudiences:0"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}