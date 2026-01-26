namespace Infrastructure.Services.Github;

public interface IGithubPersonalUserTokenService
{
    Task<string> GetTokenAsync(
        CancellationToken cancellationToken = default);

    Task ResetTokenAsync(
        CancellationToken cancellationToken = default);
}