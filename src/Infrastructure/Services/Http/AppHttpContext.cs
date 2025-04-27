using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Http;

public class AppHttpContext : IHttpContext
{
    private readonly IHttpContextAccessor _http;

    public AppHttpContext(
        IHttpContextAccessor http)
    {
        _http = http;
    }

    public CurrentUser CurrentUser => new CurrentUser(
        _http.HttpContext?.User
        ?? throw new InvalidOperationException("The Http Context does not exist"));

    public bool Exists =>
        _http.HttpContext != null;

    public bool HasUserClaims
        => Exists &&
           _http.HttpContext?.User != null &&
           _http.HttpContext.User.Claims.Any();
}