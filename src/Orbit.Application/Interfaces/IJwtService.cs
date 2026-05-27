using System.Security.Claims;

namespace Orbit.Application.Interfaces;

public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateAccessToken(Guid authUserId, Guid profileId, string username);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
