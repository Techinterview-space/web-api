using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class RegisterHandler : IRequestHandler<RegisterRequest, AuthResult>
{
    private readonly DatabaseContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITechinterviewEmailService _emailService;
    private readonly IConfiguration _configuration;

    public RegisterHandler(
        DatabaseContext context,
        IPasswordHasher passwordHasher,
        ITechinterviewEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResult> Handle(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var emailUpper = request.Email.ToUpperInvariant();
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToUpper() == emailUpper, cancellationToken);

        if (existingUser != null)
        {
            throw new Domain.Validation.Exceptions.BadRequestException("Email already registered");
        }

        var user = new User(
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            roles: Role.Interviewer);

        user.SetIdentityId($"{CurrentUser.LocalPrefix}{Guid.NewGuid():N}");

        var passwordHash = _passwordHasher.Hash(request.Password);
        user.SetPassword(passwordHash);

        var verificationToken = GenerateSecureToken();
        user.SetEmailVerificationToken(verificationToken, TimeSpan.FromHours(24));

        _context.Users.Add(user);
        await _context.TrySaveChangesAsync(cancellationToken);

        var frontendUrl = _configuration["Frontend:BaseUrl"];
        var verificationUrl = $"{frontendUrl}/verify-email?token={verificationToken}";
        await _emailService.SendEmailVerificationAsync(user, verificationUrl, cancellationToken);

        return new AuthResult
        {
            Success = true,
            Message = "Регистрация прошла успешно. Пожалуйста, проверьте вашу электронную почту для подтверждения аккаунта.",
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
