using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Features.Users.Models;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Accounts;

[ApiController]
[Route("api/account")]
[HasAnyRole]
public class AccountController : ControllerBase
{
    private readonly IAuthorization _auth;

    public AccountController(
        IAuthorization auth)
    {
        _auth = auth;
    }

    [HttpGet("me")]
    public async Task<UserAdminDto> MeAsync(
        CancellationToken cancellationToken)
    {
        var user = await _auth.CurrentUserOrFailAsync(cancellationToken);
        return new (user);
    }
}