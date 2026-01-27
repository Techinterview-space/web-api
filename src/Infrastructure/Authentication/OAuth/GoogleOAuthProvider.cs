using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects.OAuth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Authentication.OAuth;

public class GoogleOAuthProvider : IOAuthProvider
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleOAuthProvider(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public string GetAuthorizationUrl(string state, string redirectUri = null)
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        redirectUri ??= _configuration["OAuth:Google:RedirectUri"];

        return $"{AuthorizationEndpoint}" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            "&response_type=code" +
            "&scope=openid%20email%20profile" +
            $"&state={Uri.EscapeDataString(state)}" +
            "&access_type=offline" +
            "&prompt=consent";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUri = null)
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        var clientSecret = _configuration["OAuth:Google:ClientSecret"];
        redirectUri ??= _configuration["OAuth:Google:RedirectUri"];

        using var client = _httpClientFactory.CreateClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
        });

        var response = await client.PostAsync(TokenEndpoint, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OAuthTokenResponse>(json);
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string accessToken)
    {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync(UserInfoEndpoint);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var googleUser = JsonSerializer.Deserialize<GoogleUserInfo>(json);

        return new OAuthUserInfo
        {
            Id = googleUser.Sub,
            Email = googleUser.Email,
            EmailVerified = googleUser.EmailVerified,
            GivenName = googleUser.GivenName,
            FamilyName = googleUser.FamilyName,
            Name = googleUser.Name,
            Picture = googleUser.Picture,
        };
    }

    private record GoogleUserInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; init; }

        [JsonPropertyName("email")]
        public string Email { get; init; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("given_name")]
        public string GivenName { get; init; }

        [JsonPropertyName("family_name")]
        public string FamilyName { get; init; }

        [JsonPropertyName("picture")]
        public string Picture { get; init; }
    }
}
