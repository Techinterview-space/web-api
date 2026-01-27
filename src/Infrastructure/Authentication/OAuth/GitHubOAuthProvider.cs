using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects.OAuth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Authentication.OAuth;

public class GitHubOAuthProvider : IOAuthProvider
{
    private const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    private const string TokenEndpoint = "https://github.com/login/oauth/access_token";
    private const string UserInfoEndpoint = "https://api.github.com/user";
    private const string UserEmailsEndpoint = "https://api.github.com/user/emails";

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubOAuthProvider(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public string GetAuthorizationUrl(string state, string redirectUri = null)
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        redirectUri ??= _configuration["OAuth:GitHub:RedirectUri"];

        return $"{AuthorizationEndpoint}" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            "&scope=read:user%20user:email" +
            $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUri = null)
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];
        redirectUri ??= _configuration["OAuth:GitHub:RedirectUri"];

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
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
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TechInterview", "1.0"));

        var userResponse = await client.GetAsync(UserInfoEndpoint);
        userResponse.EnsureSuccessStatusCode();

        var userJson = await userResponse.Content.ReadAsStringAsync();
        var githubUser = JsonSerializer.Deserialize<GitHubUserInfo>(userJson);

        string primaryEmail = githubUser.Email;
        if (string.IsNullOrEmpty(primaryEmail))
        {
            primaryEmail = await GetPrimaryEmailAsync(client);
        }

        var nameParts = (githubUser.Name ?? githubUser.Login ?? string.Empty).Split(' ', 2);

        return new OAuthUserInfo
        {
            Id = githubUser.Id.ToString(),
            Email = primaryEmail,
            EmailVerified = !string.IsNullOrEmpty(primaryEmail),
            GivenName = nameParts.Length > 0 ? nameParts[0] : githubUser.Login,
            FamilyName = nameParts.Length > 1 ? nameParts[1] : "-",
            Name = githubUser.Name ?? githubUser.Login,
            Picture = githubUser.AvatarUrl,
        };
    }

    private async Task<string> GetPrimaryEmailAsync(HttpClient client)
    {
        try
        {
            var response = await client.GetAsync(UserEmailsEndpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var emails = JsonSerializer.Deserialize<List<GitHubEmail>>(json);

            var primaryEmail = emails?.FirstOrDefault(e => e.Primary && e.Verified);
            return primaryEmail?.Email ?? emails?.FirstOrDefault(e => e.Verified)?.Email;
        }
        catch
        {
            return null;
        }
    }

    private record GitHubUserInfo
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("login")]
        public string Login { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("email")]
        public string Email { get; init; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; init; }
    }

    private record GitHubEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; init; }

        [JsonPropertyName("primary")]
        public bool Primary { get; init; }

        [JsonPropertyName("verified")]
        public bool Verified { get; init; }
    }
}
