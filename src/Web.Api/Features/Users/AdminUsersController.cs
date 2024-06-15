using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Domain.ValueObjects.Pagination;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Users.Models;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Users;

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
    public async Task<Pageable<UserAdminDto>> All(
        [FromQuery] PageModel pageParams = null)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);

        pageParams ??= PageModel.Default;
        return await _context.Users
            .Include(x => x.UserRoles)
            .Include(x => x.Salaries)
            .Where(x => x.DeletedAt == null)
            .Select(UserAdminDto.Transformation)
            .AsPaginatedAsync(pageParams);
    }

    [HttpGet("{id:long}")]
    public async Task<UserAdminDto> GetUser(
        [FromRoute] long id)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        return await _context.Users
            .Include(x => x.UserRoles)
            .Include(x => x.Salaries)
            .Select(UserAdminDto.Transformation)
            .ByIdOrFailAsync(id);
    }

    [HttpPost("")]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        await _auth.HasRoleOrFailAsync(Role.Admin, cancellationToken);
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
    public async Task<IEnumerable<UserAdminDto>> AllInactiveAsync()
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);
        return await _context.Users
            .Include(x => x.UserRoles)
            .Where(x => x.DeletedAt != null)
            .AllAsync(x => new UserAdminDto(x));
    }
}