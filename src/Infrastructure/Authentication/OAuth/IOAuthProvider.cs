using Domain.ValueObjects.OAuth;

namespace Infrastructure.Authentication.OAuth;

public interface IOAuthProvider
{
    string GetAuthorizationUrl(string state, string redirectUri = null);

    Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUri = null);

    Task<OAuthUserInfo> GetUserInfoAsync(string accessToken);
}
