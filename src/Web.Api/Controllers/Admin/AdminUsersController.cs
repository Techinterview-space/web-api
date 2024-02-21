using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services.Users;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers.Admin;

[HasAnyRole(Role.Admin)]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public AdminUsersController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("")]
    public async Task<Pageable<UserDto>> All([FromQuery] PageModel pageParams = null)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        pageParams ??= PageModel.Default;
        return await _context.Users
            .Include(x => x.UserRoles)
            .Where(x => x.DeletedAt == null)
            .AsPaginatedAsync(x => new UserDto(x), pageParams);
    }

    [HttpGet("{id:long}")]
    public async Task<UserDto> GetUser([FromRoute] long id)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        return new UserDto(await _context.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .ByIdOrFailAsync(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        await _auth.HasRoleOrFailAsync(Role.Admin);
        await _context.Users
            .NoItemsByConditionOrFailAsync(
                x => x.Email == request.Email.ToUpper(),
                "There is a user with such email in the database",
                cancellationToken: cancellationToken);

        var user = new User(
                email: request.Email,
                firstName: request.FirstName,
                lastName: request.LastName,
                roles: request.Roles.ToArray())
            .ThrowIfInvalid();

        user = await _context.SaveAsync(user, cancellationToken);
        return Ok(user.Id);
    }

    [HttpPut("")]
    public async Task<IActionResult> Update(
        [FromBody] UserUpdateRequest request)
    {
        request.ThrowIfInvalid();

        await _auth.HasRoleOrFailAsync(Role.Admin);
        var user = await _context.Users
            .Include(x => x.UserRoles)
            .ByIdOrFailAsync(request.Id);

        user.Update(request.FirstName, request.LastName);
        if (request.HasRoles())
        {
            user.SyncRoles(request.Roles);
        }

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPut("roles")]
    public async Task<IActionResult> Update([FromBody] UserUpdateRolesRequest request)
    {
        request.ThrowIfInvalid();

        await _auth.HasRoleOrFailAsync(Role.Admin);
        var user = await _context.Users
            .Include(x => x.UserRoles)
            .ByIdOrFailAsync(request.Id);

        if (request.HasRoles())
        {
            user.SyncRoles(request.Roles);
        }

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();
        currentUser.HasAnyOrFail(Role.Admin);

        var user = (await _context.Users
                .Include(x => x.UserRoles)
                .ByIdOrFailAsync(id))
            .ActiveOrFail();

        if (currentUser.Id == user.Id)
        {
            throw new BadRequestException("You can't delete yourself");
        }

        user.Delete();

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id:long}/restore")]
    public async Task<IActionResult> Restore([FromRoute] long id)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();
        currentUser.HasAnyOrFail(Role.Admin);

        var user = (await _context.Users
                .Include(x => x.UserRoles)
                .ByIdOrFailAsync(id))
            .InactiveOrFail();

        if (currentUser.Id == user.Id)
        {
            throw new BadRequestException("You are not able to restore your own account");
        }

        user.Restore();

        await _context.TrySaveChangesAsync();
        return Ok();
    }

    [HttpGet("inactive")]
    public async Task<IEnumerable<UserDto>> AllInactiveAsync()
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        return await _context.Users
            .Include(x => x.UserRoles)
            .Where(x => x.DeletedAt != null)
            .AllAsync(x => new UserDto(x));
    }
}