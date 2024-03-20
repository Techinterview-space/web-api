using Domain.Services;

namespace Infrastructure.Authentication.Contracts;

public interface IHttpContext
{
    CurrentUser CurrentUser { get; }

    bool Exists { get; }

    bool HasUserClaims { get; }
}