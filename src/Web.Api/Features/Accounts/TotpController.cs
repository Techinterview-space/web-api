using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Accounts.Dtos;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Accounts;

[ApiController]
[Route("api/totp")]
[HasAnyRole]
public class TotpController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public TotpController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpPost("enable")]
    public async Task<SetupTotpResponse> EnableTotp(
        CancellationToken cancellationToken)
    {
        var user = await _auth.GetCurrentUserOrFailAsync(cancellationToken);
        if (user.IsMfaEnabled())
        {
            throw new BadRequestException("TOTP MFA is already enabled for this user.");
        }

        if (!user.Has(Role.Admin))
        {
            throw new BadRequestException("Only admin users can enable TOTP MFA.");
        }

        user.GenerateTotpSecretKey();
        await _context.TrySaveChangesAsync(cancellationToken);

        return new SetupTotpResponse(user);
    }

    [HttpPost("disable")]
    public async Task<SetupTotpResponse> DisableTotp(
        CancellationToken cancellationToken)
    {
        var user = await _auth.GetCurrentUserOrFailAsync(cancellationToken);
        if (!user.IsMfaEnabled())
        {
            throw new BadRequestException("TOTP MFA is already enabled for this user.");
        }

        user.DisableTotp();
        await _context.TrySaveChangesAsync(cancellationToken);

        return new SetupTotpResponse(user);
    }

    [HttpPost("verify")]
    public async Task<CheckTotpResponse> VerifyTotp(
        [FromBody] CheckTotpRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _auth.GetCurrentUserOrFailAsync(cancellationToken);
        if (!user.IsMfaEnabled())
        {
            throw new BadRequestException("TOTP MFA is not enabled for this user.");
        }

        var verifyResult = user.VerifyTotp(request.TotpCode);
        if (verifyResult)
        {
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        return new CheckTotpResponse(verifyResult);
    }
}