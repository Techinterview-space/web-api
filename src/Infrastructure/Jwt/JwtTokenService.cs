using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Jwt;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();

        var secret = _configuration["OAuth:Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.IdentityId ?? $"user|{user.Id}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("email_verified", user.EmailConfirmed.ToString().ToLowerInvariant()),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim("token_type", "user"),
        };

        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            claims.Add(new Claim("picture", user.ProfilePicture));
        }

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.RoleId.ToString()));
        }

        return GenerateToken(claims, GetAccessTokenExpirationMinutes());
    }

    public string GenerateAccessToken(M2mClient client, string[] requestedScopes)
    {
        var grantedScopes = client.Scopes
            .Select(s => s.Scope)
            .Where(s => requestedScopes.Length == 0 || requestedScopes.Contains(s))
            .ToArray();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, $"m2m|{client.ClientId}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("client_name", client.Name),
            new Claim("token_type", "m2m"),
        };

        foreach (var scope in grantedScopes)
        {
            claims.Add(new Claim("scope", scope));
        }

        return GenerateToken(claims, GetM2mTokenExpirationMinutes());
    }

    public RefreshToken GenerateRefreshToken(long userId, string deviceInfo = null)
    {
        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);

        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty);

        var tokenExpirationDays = GetRefreshTokenExpirationDays();
        return new RefreshToken(
            userId: userId,
            token: token,
            expiresAt: DateTimeOffset.UtcNow.AddDays(tokenExpirationDays),
            deviceInfo: deviceInfo);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["OAuth:Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["OAuth:Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateToken(List<Claim> claims, int expirationMinutes)
    {
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["OAuth:Jwt:Issuer"],
            audience: _configuration["OAuth:Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    private int GetAccessTokenExpirationMinutes() =>
        int.TryParse(_configuration["OAuth:Jwt:AccessTokenExpirationMinutes"], out var minutes) ? minutes : 60;

    private int GetM2mTokenExpirationMinutes() =>
        int.TryParse(_configuration["OAuth:Jwt:M2mTokenExpirationMinutes"], out var minutes) ? minutes : 60;

    private int GetRefreshTokenExpirationDays()
    {
        var configValue = _configuration["OAuth:Jwt:RefreshTokenExpirationDays"] ?? "10";
        return int.TryParse(configValue, out var days) ? days : 30;
    }
}
