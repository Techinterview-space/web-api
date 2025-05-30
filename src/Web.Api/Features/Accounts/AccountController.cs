using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Accounts.Dtos;
using Web.Api.Features.Users.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Accounts;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthorization auth,
        DatabaseContext context,
        ILogger<AccountController> logger)
    {
        _auth = auth;
        _context = context;
        _logger = logger;
    }

    [HttpGet("me")]
    [HasAnyRole]
    public async Task<UserDto> Me(
        CancellationToken cancellationToken)
    {
        var user = await _auth.GetCurrentUserOrNullAsync(cancellationToken);
        if (user == null)
        {
            throw new AuthenticationException("The current user is not authenticated");
        }

        if (user.IsMfaEnabled() &&
            user.TotpVerificationExpired())
        {
            throw new AuthenticationException("TOTP verification is expired");
        }

        return new UserDto(user);
    }

    [HttpGet("my-salaries")]
    [HasAnyRole]
    public async Task<List<UserSalaryDto>> GetMySalaries(
        CancellationToken cancellationToken)
    {
        var user = _auth.CurrentUser;
        if (user == null)
        {
            throw new AuthenticationException("The current user is not authenticated");
        }

        var emailUpper = user.EmailUpper;
        var salaries = await _context.Salaries
            .Include(x => x.User)
            .Where(x => x.User.Email.ToUpper() == emailUpper)
            .Select(UserSalaryDto.Transform)
            .ToListAsync(cancellationToken);

        return salaries;
    }

    [HttpGet("check-totp")]
    public async Task<CheckTotpRequiredResponse> CheckTotp(
        CancellationToken cancellationToken)
    {
        if (!_auth.HasUserClaims)
        {
            return new CheckTotpRequiredResponse
            {
                IsMfaEnabled = false,
            };
        }

        var currentUser = _auth.CurrentUser;

        var upperEmail = currentUser.Email.ToUpperInvariant();
        var user = await _context.Users
            .Select(x => new CheckTotpRequiredResponse
            {
                Id = x.Id,
                Email = x.Email,
                IsMfaEnabled = x.TotpSecret != null,
            })
            .FirstOrDefaultAsync(x => x.Email.ToUpper() == upperEmail, cancellationToken);

        if (user != null)
        {
            return user;
        }

        var newUser = await _auth.GetOrCreateAsync(cancellationToken);
        return new CheckTotpRequiredResponse
        {
            Id = newUser.Id,
            Email = newUser.Email,
            IsMfaEnabled = newUser.TotpSecret != null,
        };
    }

    [HttpPost("unsubscribe-me/{token}")]
    public async Task<IActionResult> UnsubscribeMe(
        [FromRoute] string token,
        CancellationToken cancellationToken)
    {
        token = token?.Trim();

        var user = await _context.Users.FirstOrDefaultAsync(
            x => x.UniqueToken == token,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Unsubscribe attempt with invalid token: {Token}",
                token);

            return Ok(new
            {
                result = true,
            });
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BadRequestException("Invalid token");
        }

        user.UnsubscribeFromAll();
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            result = true,
        });
    }
}