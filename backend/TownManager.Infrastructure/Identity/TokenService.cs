using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Identity;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _issuer = configuration["Jwt:Issuer"]!;
    private readonly string _audience = configuration["Jwt:Audience"]!;
    private readonly SymmetricSecurityKey _key = new(
        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

    public string GenerateAccessToken(string userId, string email, IList<string>? roles = null)
    {
        var token = CreateToken(userId, email, DateTime.UtcNow.AddHours(1), roles);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(string userId, string email)
    {
        var token = CreateToken(userId, email, DateTime.UtcNow.AddDays(7));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            }, out _);
        }
        catch
        {
            return null;
        }
    }

    private JwtSecurityToken CreateToken(string userId, string email, DateTime expires, IList<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
        };
        if (roles is not null)
            foreach (var r in roles)
                claims.Add(new(ClaimTypes.Role, r));

        return new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        );
    }
}
