using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authentication.OAuth;

public interface IOAuthProviderFactory
{
    IOAuthProvider GetProvider(string providerName);
}

public class OAuthProviderFactory : IOAuthProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public OAuthProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOAuthProvider GetProvider(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "google" => _serviceProvider.GetRequiredService<GoogleOAuthProvider>(),
            "github" => _serviceProvider.GetRequiredService<GitHubOAuthProvider>(),
            _ => throw new ArgumentException($"Unknown OAuth provider: {providerName}", nameof(providerName)),
        };
    }
}
