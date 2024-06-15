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

    public UsersController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("{id:long}")]
    public async Task<UserAdminDto> UserAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync(cancellationToken);
        if (currentUser.Has(Role.Admin) || currentUser.Id == id)
        {
            return await _context.Users
                .Include(x => x.UserRoles)
                .Include(x => x.Salaries)
                .Select(UserAdminDto.Transformation)
                .ByIdOrFailAsync(id, cancellationToken);
        }

        throw new NoPermissionsException("You do not have permission to view this user");
    }
}