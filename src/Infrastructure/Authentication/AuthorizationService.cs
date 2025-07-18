﻿using System.Security.Authentication;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public record AuthorizationService : IAuthorization
{
    private readonly IHttpContext _http;
    private readonly DatabaseContext _context;
    private readonly ILogger<AuthorizationService> _logger;

    private User _userFromDatabase;

    public AuthorizationService(
        IHttpContext http,
        DatabaseContext context,
        ILogger<AuthorizationService> logger)
    {
        if (!http.Exists)
        {
            throw new InvalidOperationException("Not allowed out of web");
        }

        _http = http;
        _context = context;
        _logger = logger;
    }

    public async Task<User> GetCurrentUserOrFailAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetCurrentUserOrNullAsync(cancellationToken)
            ?? throw new AuthenticationException("The current user is not authenticated");
    }

    public async Task<User> GetCurrentUserOrNullAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_http.HasUserClaims)
        {
            return null;
        }

        return _userFromDatabase ??= await GetOrCreateAsync(cancellationToken);
    }

    public CurrentUser CurrentUser
        => _http.CurrentUser;

    public bool HasUserClaims
        => _http.HasUserClaims;

    public async Task HasRoleOrFailAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        (await GetCurrentUserOrNullAsync(cancellationToken)).HasOrFail(role);
    }

    public async Task HasAnyRoleOrFailAsync(params Role[] roles)
    {
        (await GetCurrentUserOrNullAsync()).HasAnyOrFail(roles);
    }

    public async Task<User> GetOrCreateAsync(
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(x => x.UserRoles)
            .Include(x => x.Salaries)
            .ByEmailOrIdentityIdOrNullAsync(
                _http.CurrentUser.Email,
                _http.CurrentUser.UserId,
                cancellationToken);

        _logger.LogInformation(
            "Trying to get user by email: {Email} or identityId: {IdentityId}. Result: {UserId}",
            _http.CurrentUser.Email,
            _http.CurrentUser.UserId,
            user?.Id.ToString() ?? "null");

        if (user == null)
        {
            var claimsUser = new User(_http.CurrentUser);
            user = await _context.AddEntityAsync(claimsUser, cancellationToken);
            await _context.TrySaveChangesAsync(cancellationToken);
            return user;
        }

        if (!user.CheckIdentity(_http.CurrentUser))
        {
            _logger.LogError(
                "User {Email} is not authenticated. " +
                "Id: {UserId}," +
                "IdentityId: {IdentityId}, " +
                "Auth0 Id: {AuthId}",
                user.Email,
                user.Id,
                user.IdentityId,
                _http.CurrentUser.UserId);

            throw new AuthenticationException("User tries to use incorrect account to login");
        }

        user.UpdateData(_http.CurrentUser);
        user.RenewLastLoginTime();

        await _context.TrySaveChangesAsync(cancellationToken);

        return user;
    }
}