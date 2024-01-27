using Domain.Services;

namespace Domain.Authentication.Abstract;

public interface IHttpContext
{
    CurrentUser CurrentUser { get; }

    bool Exists { get; }

    bool HasUserClaims { get; }
}