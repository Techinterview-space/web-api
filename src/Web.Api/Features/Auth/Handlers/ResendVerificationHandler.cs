using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class ResendVerificationHandler : IRequestHandler<ForgotPasswordRequest, AuthResult>
{
    private readonly DatabaseContext _context;
    private readonly ITechinterviewEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ResendVerificationHandler(
        DatabaseContext context,
        ITechinterviewEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResult> Handle(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var emailUpper = request.Email.ToUpperInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToUpper() == emailUpper, cancellationToken);

        if (user == null)
        {
            return new AuthResult
            {
                Success = true,
                Message = "If your email is registered and not yet verified, you will receive a verification link.",
            };
        }

        if (user.EmailConfirmed)
        {
            return new AuthResult
            {
                Success = true,
                Message = "If your email is registered and not yet verified, you will receive a verification link.",
            };
        }

        var verificationToken = GenerateSecureToken();
        user.SetEmailVerificationToken(verificationToken, TimeSpan.FromHours(24));
        await _context.TrySaveChangesAsync(cancellationToken);

        var frontendUrl = _configuration["Frontend:BaseUrl"];
        var verificationUrl = $"{frontendUrl}/verify-email?token={verificationToken}";
        await _emailService.SendEmailVerificationAsync(user, verificationUrl, cancellationToken);

        return new AuthResult
        {
            Success = true,
            Message = "If your email is registered and not yet verified, you will receive a verification link.",
        };
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty);
    }
}
