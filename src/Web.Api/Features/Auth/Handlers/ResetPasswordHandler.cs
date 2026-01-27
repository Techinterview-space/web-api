using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, AuthResult>
{
    private readonly DatabaseContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordHandler(
        DatabaseContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken);

        if (user == null)
        {
            throw new BadRequestException("Invalid reset token");
        }

        if (user.PasswordResetTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new BadRequestException("Reset token has expired");
        }

        var passwordHash = _passwordHasher.Hash(request.NewPassword);
        user.SetPassword(passwordHash);
        user.ClearPasswordResetToken();
        user.ResetFailedLoginAttempts();

        var refreshTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.Revoke();
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            Success = true,
            Message = "Password reset successfully. You can now log in with your new password.",
        };
    }
}
