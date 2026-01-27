using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication.OAuth;
using Infrastructure.Database;
using Infrastructure.Jwt;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public record GoogleCallbackRequest
{
    public string Code { get; init; }

    public string DeviceInfo { get; init; }
}

public class GoogleCallbackHandler
{
    private readonly GoogleOAuthProvider _googleProvider;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly DatabaseContext _context;

    public GoogleCallbackHandler(
        GoogleOAuthProvider googleProvider,
        IJwtTokenService jwtTokenService,
        DatabaseContext context)
    {
        _googleProvider = googleProvider;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<AuthTokenResponse> HandleAsync(
        GoogleCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var googleTokens = await _googleProvider.ExchangeCodeAsync(request.Code);
        var googleUser = await _googleProvider.GetUserInfoAsync(googleTokens.AccessToken);

        var identityId = $"{CurrentUser.GooglePrefix}{googleUser.Id}";
        var emailUpper = googleUser.Email.ToUpperInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(
                u => u.Email.ToUpper() == emailUpper,
                cancellationToken);

        if (user == null)
        {
            var hasAnyOtherUsers = await _context.Users.AnyAsync(cancellationToken);
            user = User.CreateFromExternalProviderAuth(
                email: googleUser.Email,
                firstName: googleUser.GivenName ?? googleUser.Email.Split('@')[0],
                lastName: googleUser.FamilyName ?? "-",
                identityId: identityId,
                profilePicture: googleUser.Picture,
                roles: hasAnyOtherUsers
                    ? new List<Role> { Role.Interviewer, }
                    : new List<Role> { Role.Interviewer, Role.Admin, });

            _context.Users.Add(user);
            await _context.TrySaveChangesAsync(cancellationToken);
        }
        else
        {
            if (string.IsNullOrEmpty(user.IdentityId))
            {
                user.SetIdentityId(identityId);
            }

            user.SetProfilePicture(googleUser.Picture);
            user.RecordLogin();
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(
            user.Id,
            request.DeviceInfo);

        if (refreshToken is not null)
        {
            _context.Set<RefreshToken>().Add(refreshToken);
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        return new AuthTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken?.Token,
            ExpiresIn = 3600,
            TokenType = "Bearer",
        };
    }
}
