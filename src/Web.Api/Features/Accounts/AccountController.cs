using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Database.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Accounts.Dtos;
using Web.Api.Features.Users.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Accounts;

[ApiController]
[Route("api/account")]
[HasAnyRole]
public class AccountController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public AccountController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<UserAdminDto> Me(
        CancellationToken cancellationToken)
    {
        var user = await _auth.CurrentUserOrNullAsync(cancellationToken);
        if (user == null)
        {
            throw new AuthenticationException("The current user is not authenticated");
        }

        if (user.IsMfaEnabled() &&
            user.TotpVerificationExpired())
        {
            throw new AuthenticationException("TOTP verification is expired");
        }

        return new UserAdminDto(user);
    }

    [HttpGet("check-totp")]
    public async Task<CheckTotpRequiredResponse> CheckTotp(
        CancellationToken cancellationToken)
    {
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

        if (user == null)
        {
            throw new AuthenticationException("The current user is not authenticated");
        }

        return user;
    }
}