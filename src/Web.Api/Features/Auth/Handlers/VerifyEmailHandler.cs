using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class VerifyEmailHandler : IRequestHandler<string, AuthResult>
{
    private readonly DatabaseContext _context;

    public VerifyEmailHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<AuthResult> Handle(
        string token,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, cancellationToken);

        if (user == null)
        {
            throw new BadRequestException("Invalid verification token");
        }

        if (user.EmailVerificationTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new BadRequestException("Verification token has expired");
        }

        user.ConfirmEmail();
        user.ClearEmailVerificationToken();
        await _context.TrySaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            Success = true,
            Message = "Email verified successfully. You can now log in.",
        };
    }
}
