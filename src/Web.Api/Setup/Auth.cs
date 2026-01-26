using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Web.Api.Setup;

public static class Auth
{
    private const string InternalJwtScheme = "InternalJwt";

    public static IServiceCollection SetupAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecret = configuration["OAuth:Jwt:Secret"];
        var hasInternalJwt = !string.IsNullOrEmpty(jwtSecret) &&
                             jwtSecret != "YOUR_JWT_SECRET_KEY_MIN_32_CHARACTERS_LONG";

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        if (hasInternalJwt)
        {
            authBuilder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["OAuth:Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["OAuth:Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });
        }
        else
        {
            authBuilder.AddJwtBearer(options =>
            {
                options.Authority = configuration["IdentityServer:Authority"];
                options.Audience = configuration["IdentityServer:Audience"];
            });
        }

        return services;
    }
}