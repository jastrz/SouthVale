using TownManager.Domain.Entities;

namespace TownManager.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string email);
}