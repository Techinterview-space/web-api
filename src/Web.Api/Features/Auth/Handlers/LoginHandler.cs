using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Jwt;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class LoginHandler : IRequestHandler<LoginRequest, AuthTokenResponse>
{
    private readonly DatabaseContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(
        DatabaseContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthTokenResponse> Handle(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var emailUpper = request.Email.ToUpperInvariant();
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email.ToUpper() == emailUpper, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (user.IsLockedOut)
        {
            throw new UnauthorizedException($"Account is locked. Try again after {user.LockedUntil:HH:mm}");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedException("This account uses social login. Please sign in with Google or GitHub.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.IncrementFailedLoginAttempts();
            await _context.TrySaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid email or password");
        }

        user.ResetFailedLoginAttempts();
        user.RecordLogin();

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

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
