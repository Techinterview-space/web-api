using System.Threading;
using System.Threading.Tasks;
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

    [HttpPost("setup")]
    public async Task<SetupTotpResponse> SetupTotp(
        CancellationToken cancellationToken)
    {
        var user = await _auth.CurrentUserOrFailAsync(cancellationToken);
        if (user.IsMfaEnabled())
        {
            throw new BadRequestException("TOTP MFA is already enabled for this user.");
        }

        user.GenerateTotpSecretKey();
        await _context.TrySaveChangesAsync(cancellationToken);

        return new SetupTotpResponse(user);
    }

    [HttpPost("verify")]
    public async Task<CheckTotpResponse> VerifyTotp(
        [FromBody] CheckTotpRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _auth.CurrentUserOrFailAsync(cancellationToken);
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