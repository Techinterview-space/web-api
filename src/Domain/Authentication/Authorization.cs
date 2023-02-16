using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services;

namespace Domain.Authentication;

public class Authorization : IAuthorization
{
    private readonly IHttpContext _http;
    private readonly DatabaseContext _context;
    private readonly bool _withinBackgroundJob;

    private User _userFromDatabase;

    public Authorization(IHttpContext http, DatabaseContext context)
    {
        _http = http;
        _context = context;
        _withinBackgroundJob = !_http.Exists;
    }

    public async Task<User> CurrentUserAsync()
    {
        if (_withinBackgroundJob)
        {
            throw new InvalidOperationException("The current user is not available within background class");
        }

        return _userFromDatabase ??= await new CurrentUserProvider(_context, _http.CurrentUser).GetOrCreateAsync();
    }

    public async Task<IReadOnlyCollection<Guid>> MyOrganizationsAsync()
    {
        var user = await CurrentUserAsync();
        return user.OrganizationUsers.Select(x => x.OrganizationId).ToArray();
    }

    public async Task<bool> IsMyOrganizationAsync(Guid organizationId)
    {
        var user = await CurrentUserAsync();
        return user.IsMyOrganization(organizationId);
    }

    public CurrentUser CurrentUser
    {
        get
        {
            if (_withinBackgroundJob)
            {
                throw new InvalidOperationException("The current user is not available within background class");
            }

            return _http.CurrentUser;
        }
    }

    public async Task HasRoleOrFailAsync(Role role)
    {
        (await CurrentUserAsync()).HasOrFail(role);
    }

    public async Task HasAnyRoleOrFailAsync(params Role[] roles)
    {
        (await CurrentUserAsync()).HasAnyOrFail(roles);
    }
}