using System.Security.Claims;
using Domain.Entities.Auth;
using Domain.Entities.Users;

namespace Infrastructure.Jwt;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);

    string GenerateAccessToken(M2mClient client, string[] requestedScopes);

    RefreshToken GenerateRefreshToken(long userId, string deviceInfo = null);

    ClaimsPrincipal ValidateToken(string token);
}
