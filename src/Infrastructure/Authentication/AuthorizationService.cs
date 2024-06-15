using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;

namespace Infrastructure.Authentication;

public record AuthorizationService : IAuthorization
{
    private readonly IHttpContext _http;
    private readonly DatabaseContext _context;

    private User _userFromDatabase;

    public AuthorizationService(
        IHttpContext http,
        DatabaseContext context)
    {
        if (!http.Exists)
        {
            throw new InvalidOperationException("Not allowed out of web");
        }

        _http = http;
        _context = context;
    }

    public async Task<User> CurrentUserOrFailAsync(
        CancellationToken cancellationToken = default)
    {
        return await CurrentUserOrNullAsync(cancellationToken)
            ?? throw new InvalidOperationException("The current user is not authenticated");
    }

    public async Task<User> CurrentUserOrNullAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_http.HasUserClaims)
        {
            return null;
        }

        return _userFromDatabase
            ??= await new CurrentUserProvider(_context, _http.CurrentUser)
                .GetOrCreateAsync();
    }

    public CurrentUser CurrentUser
        => _http.CurrentUser;

    public async Task HasRoleOrFailAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        (await CurrentUserOrNullAsync(cancellationToken)).HasOrFail(role);
    }

    public async Task HasAnyRoleOrFailAsync(params Role[] roles)
    {
        (await CurrentUserOrNullAsync()).HasAnyOrFail(roles);
    }
}