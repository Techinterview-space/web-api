using Domain.Database;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services;
using Infrastructure.Authentication.Contracts;

namespace Infrastructure.Authentication;

public record AuthorizationService : IAuthorization
{
    private readonly IHttpContext _http;
    private readonly DatabaseContext _context;
    private readonly bool _withinBackgroundJob;

    private User _userFromDatabase;

    public AuthorizationService(
        IHttpContext http,
        DatabaseContext context)
    {
        _http = http;
        _context = context;
        _withinBackgroundJob = !_http.Exists;
    }

    public async Task<User> CurrentUserOrFailAsync()
    {
        return await CurrentUserOrNullAsync()
            ?? throw new InvalidOperationException("The current user is not authenticated");
    }

    public async Task<User> CurrentUserOrNullAsync()
    {
        if (_withinBackgroundJob)
        {
            throw new InvalidOperationException("The current user is not available within background class");
        }

        if (!_http.HasUserClaims)
        {
            return null;
        }

        return _userFromDatabase ??= await new CurrentUserProvider(_context, _http.CurrentUser).GetOrCreateAsync();
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
        (await CurrentUserOrNullAsync()).HasOrFail(role);
    }

    public async Task HasAnyRoleOrFailAsync(params Role[] roles)
    {
        (await CurrentUserOrNullAsync()).HasAnyOrFail(roles);
    }
}