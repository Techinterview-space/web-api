using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Users;
using MG.Utils.EFCore;
using MG.Utils.Entities;
using MG.Utils.Exceptions;
using MG.Utils.Pagination;
using MG.Utils.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<Pageable<UserDto>> AllAsync([FromQuery] PageModel pageParams = null)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        pageParams ??= PageModel.Default;
        return await _context.Users
            .Include(x => x.UserRoles)
            .Where(x => x.DeletedAt == null)
            .AsPaginatedAsync(x => new UserDto(x), pageParams);
    }

    [HttpGet("{id:long}")]
    public async Task<UserDto> UserAsync([FromRoute] long id)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        return new UserDto(await _context.Users
            .Include(x => x.UserRoles)
            .Include(x => x.OrganizationUsers)
            .ThenInclude(x => x.Organization)
            .AsNoTracking()
            .ByIdOrFailAsync(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateUserRequest request)
    {
        request.ThrowIfInvalid();

        await _auth.HasRoleOrFailAsync(Role.Admin);
        await _context.Users.NoItemsByConditionOrFailAsync(
            x => x.Email == request.Email.ToUpper(),
            "There is a user with such email in the database");

        var user = new User(
                email: request.Email,
                firstName: request.FirstName,
                lastName: request.LastName,
                roles: request.Roles.ToArray())
            .ThrowIfInvalid();

        var id = await _context.SaveAsync(user);

        return Ok(id);
    }

    [HttpPut("")]
    public async Task<IActionResult> UpdateAsync([FromBody] UserUpdateRequest request)
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
    public async Task<IActionResult> UpdateAsync([FromBody] UserUpdateRolesRequest request)
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
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        var currentUser = await _auth.CurrentUserAsync();
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
    public async Task<IActionResult> RestoreAsync([FromRoute] long id)
    {
        var currentUser = await _auth.CurrentUserAsync();
        currentUser.HasAnyOrFail(Role.Admin);

        var user = (await _context.Users
                .Include(x => x.UserRoles)
                .ByIdOrFailAsync(id))
            .InactiveOrFail();

        if (currentUser.Id == user.Id)
        {
            throw new BadRequestException("Нельзя восстановить свою учетную запись");
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