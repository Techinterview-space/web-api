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
using Web.Api.Features.Users.SearchUsersForAdmin;
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
    public async Task<Pageable<UserDto>> All(
        [FromQuery] SearchUsersForAdminQueryParams queryParams = null)
    {
        await _auth.HasRoleOrFailAsync(Role.Admin);

        queryParams ??= new SearchUsersForAdminQueryParams();

        var emailFilter = queryParams.Email?.Trim().ToLowerInvariant();
        return await _context.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
            .Include(x => x.Salaries)
            .Where(x => x.DeletedAt == null)
            .When(queryParams.HasEmailFilter(), x => x.Email != null && x.Email.ToLower().Contains(emailFilter))
            .When(queryParams.HasUnsubscribeFilter(), x => x.UnsubscribeMeFromAll == queryParams.UnsubscribeMeFromAll.Value)
            .OrderBy(x => x.CreatedAt)
            .Select(UserDto.Transformation)
            .AsPaginatedAsync(queryParams);
    }

    [HttpGet("{id:long}")]
    public async Task<UserDto> GetUser(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(x => x.UserRoles)
            .Include(x => x.Salaries)
            .Select(UserDto.Transformation)
            .ByIdOrFailAsync(id, cancellationToken: cancellationToken);
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
        [FromBody] UserUpdateRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        var user = await _context.Users
            .Include(x => x.UserRoles)
            .ByIdOrFailAsync(request.Id, cancellationToken: cancellationToken);

        user.Update(request.FirstName, request.LastName);
        if (request.HasRoles())
        {
            user.SyncRoles(request.Roles);
        }

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("roles")]
    public async Task<IActionResult> Update(
        [FromBody] UserUpdateRolesRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        var user = await _context.Users
            .Include(x => x.UserRoles)
            .ByIdOrFailAsync(request.Id, cancellationToken: cancellationToken);

        if (request.HasRoles())
        {
            user.SyncRoles(request.Roles);
        }

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.GetCurrentUserOrNullAsync(cancellationToken);
        currentUser.HasAnyOrFail(Role.Admin);

        var user = (await _context.Users
                .Include(x => x.UserRoles)
                .ByIdOrFailAsync(id, cancellationToken: cancellationToken))
            .ActiveOrFail();

        if (currentUser.Id == user.Id)
        {
            throw new BadRequestException("You can't delete yourself");
        }

        user.Delete();

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("{id:long}/restore")]
    public async Task<IActionResult> Restore(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.GetCurrentUserOrNullAsync(cancellationToken);
        currentUser.HasAnyOrFail(Role.Admin);

        var user = (await _context.Users
                .Include(x => x.UserRoles)
                .ByIdOrFailAsync(id, cancellationToken))
            .InactiveOrFail();

        if (currentUser.Id == user.Id)
        {
            throw new BadRequestException("You are not able to restore your own account");
        }

        user.Restore();

        await _context.TrySaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("inactive")]
    public async Task<IEnumerable<UserDto>> AllInactiveAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(x => x.UserRoles)
            .Where(x => x.DeletedAt != null)
            .Select(UserDto.Transformation)
            .ToListAsync(cancellationToken);
    }
}