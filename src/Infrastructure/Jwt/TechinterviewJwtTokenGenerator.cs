using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Jwt;

public record TechinterviewJwtTokenGenerator
{
    private readonly string _secretKey;

    private string _result;

    public TechinterviewJwtTokenGenerator(
        string secretKey)
    {
        _secretKey = secretKey;
    }

    public override string ToString()
    {
        return _result ??= GenerateInternal();
    }

    private string GenerateInternal()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, "TechInterview.space Bot"),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        // Serialize token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}