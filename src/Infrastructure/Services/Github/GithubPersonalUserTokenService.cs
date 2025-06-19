using System.Diagnostics;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Github;

public class GithubPersonalUserTokenService : IGithubPersonalUserTokenService
{
    public const string MemCacheKey = nameof(GithubPersonalUserTokenService) + "__Token";

    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<GithubPersonalUserTokenService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public GithubPersonalUserTokenService(
        DatabaseContext databaseContext,
        ILogger<GithubPersonalUserTokenService> logger,
        IMemoryCache memoryCache,
        IConfiguration configuration)
    {
        _databaseContext = databaseContext;
        _logger = logger;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    public Task<string> GetTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (Debugger.IsAttached)
        {
            var tokenFromConfig = Environment.GetEnvironmentVariable("Telegram__GithubPATForLocalDevelopment");
            _logger.LogInformation(
                "Token from config: {tokenFromConfig}",
                tokenFromConfig);

            return Task.FromResult(tokenFromConfig);
        }

        return _memoryCache.GetOrCreateAsync(
            MemCacheKey,
            async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
                entry.Priority = CacheItemPriority.Normal;

                return await GetTokenFromDatabaseAsync(cancellationToken);
            });
    }

    public async Task ResetTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (Debugger.IsAttached)
        {
            return;
        }

        _memoryCache.Remove(MemCacheKey);
        await _memoryCache.GetOrCreateAsync(
            MemCacheKey,
            async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
                entry.Priority = CacheItemPriority.Normal;

                return await GetTokenFromDatabaseAsync(cancellationToken);
            });
    }

    private async Task<string> GetTokenFromDatabaseAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.AddHours(1);
        var token = await _databaseContext.GithubPersonalUserTokens
            .Where(x => x.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null)
        {
            _logger.LogError("No github PAT was not found");
            throw new InvalidOperationException("No github PAT was found");
        }

        return token.Token;
    }
}