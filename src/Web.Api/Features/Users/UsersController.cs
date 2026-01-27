using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Users.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Users;

[HasAnyRole]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public UsersController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("{id:long}")]
    public async Task<UserDto> GetUser(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var currentUser = _auth.CurrentUser;
        var currentUserEmail = currentUser.Email.ToLowerInvariant();

        var currentUserId = await _context.Users
            .Where(x => x.Email == currentUserEmail)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentUser.Has(Role.Admin) && id != currentUserId)
        {
            throw new NoPermissionsException("You do not have permission to view this user");
        }

        var user = await _context.Users
            .Include(x => x.UserRoles)
            .IncludeWhen(currentUserId == id, x => x.Salaries)
            .ByIdOrFailAsync(id, cancellationToken);

        return new UserDto(user);
    }
}