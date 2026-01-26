using System;
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

public record GitHubCallbackRequest
{
    public string Code { get; init; }

    public string DeviceInfo { get; init; }
}

public class GitHubCallbackHandler
{
    private readonly GitHubOAuthProvider _gitHubProvider;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly DatabaseContext _context;

    public GitHubCallbackHandler(
        GitHubOAuthProvider gitHubProvider,
        IJwtTokenService jwtTokenService,
        DatabaseContext context)
    {
        _gitHubProvider = gitHubProvider;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<AuthTokenResponse> HandleAsync(
        GitHubCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var githubTokens = await _gitHubProvider.ExchangeCodeAsync(request.Code);
        var githubUser = await _gitHubProvider.GetUserInfoAsync(githubTokens.AccessToken);

        var identityId = $"{CurrentUser.GithubPrefix}{githubUser.Id}";
        var emailUpper = githubUser.Email?.ToUpperInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(
                u => u.IdentityId == identityId ||
                     (u.IdentityId == null && !string.IsNullOrEmpty(emailUpper) && u.Email.ToUpper() == emailUpper),
                cancellationToken);

        if (user == null)
        {
            user = new User(
                email: githubUser.Email ?? $"{githubUser.Id}@github.placeholder",
                firstName: githubUser.GivenName ?? githubUser.Name ?? "GitHub",
                lastName: githubUser.FamilyName ?? "User",
                roles: Role.Interviewer);
            user.SetIdentityId(identityId);
            user.ConfirmEmail();
            user.SetProfilePicture(githubUser.Picture);

            _context.Users.Add(user);
            await _context.TrySaveChangesAsync(cancellationToken);
        }
        else
        {
            if (string.IsNullOrEmpty(user.IdentityId))
            {
                user.SetIdentityId(identityId);
            }

            user.SetProfilePicture(githubUser.Picture);
            user.RecordLogin();
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id, request.DeviceInfo);

        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new AuthTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = 3600,
            TokenType = "Bearer",
        };
    }
}
