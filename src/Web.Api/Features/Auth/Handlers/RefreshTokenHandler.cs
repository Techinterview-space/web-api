using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Jwt;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, AuthTokenResponse>
{
    private readonly DatabaseContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenHandler(
        DatabaseContext context,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthTokenResponse> Handle(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var storedToken = await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (storedToken.IsRevoked)
        {
            throw new UnauthorizedException("Refresh token has been revoked");
        }

        if (storedToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedException("Refresh token has expired");
        }

        storedToken.Revoke();

        var user = storedToken.User;
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id, storedToken.DeviceInfo);

        _context.Set<RefreshToken>().Add(newRefreshToken);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new AuthTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = 3600,
            TokenType = "Bearer",
        };
    }
}
