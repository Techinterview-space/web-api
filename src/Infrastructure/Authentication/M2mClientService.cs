using System.Security.Cryptography;
using Domain.Entities.Auth;
using Domain.Enums;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public interface IM2mClientService
{
    Task<M2mClient> ValidateClientCredentialsAsync(string clientId, string clientSecret);

    Task<(M2mClient Client, string PlainSecret)> CreateClientAsync(
        string name,
        string description,
        string[] scopes,
        long createdByUserId);

    Task<string> RegenerateSecretAsync(long clientId);

    Task<M2mClient> GetByClientIdAsync(string clientId);

    Task<M2mClient> GetByIdAsync(long id);

    Task<List<M2mClient>> GetAllAsync();

    Task RecordUsageAsync(string clientId, string ipAddress);

    Task UpdateScopesAsync(long clientId, string[] scopes);

    Task DeactivateAsync(long clientId);

    Task ActivateAsync(long clientId);

    Task DeleteAsync(long clientId);
}

public class M2mClientService : IM2mClientService
{
    private readonly DatabaseContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public M2mClientService(
        DatabaseContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<M2mClient> ValidateClientCredentialsAsync(string clientId, string clientSecret)
    {
        var client = await _context.Set<M2mClient>()
            .Include(c => c.Scopes)
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive && c.DeletedAt == null);

        if (client == null)
        {
            return null;
        }

        if (!_passwordHasher.Verify(clientSecret, client.ClientSecretHash))
        {
            return null;
        }

        return client;
    }

    public async Task<(M2mClient Client, string PlainSecret)> CreateClientAsync(
        string name,
        string description,
        string[] scopes,
        long createdByUserId)
    {
        var client = new M2mClient(name, description, createdByUserId);

        var plainSecret = GenerateSecureSecret();
        client.SetClientSecret(_passwordHasher.Hash(plainSecret));

        _context.Set<M2mClient>().Add(client);
        await _context.TrySaveChangesAsync();

        foreach (var scope in scopes.Where(M2mScope.IsValidScope))
        {
            var clientScope = new M2mClientScope(client.Id, scope);
            _context.Set<M2mClientScope>().Add(clientScope);
        }

        await _context.TrySaveChangesAsync();

        return (client, plainSecret);
    }

    public async Task<string> RegenerateSecretAsync(long clientId)
    {
        var client = await _context.Set<M2mClient>()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.DeletedAt == null);

        if (client == null)
        {
            throw new InvalidOperationException($"M2M client with ID {clientId} not found");
        }

        var plainSecret = GenerateSecureSecret();
        client.SetClientSecret(_passwordHasher.Hash(plainSecret));
        await _context.TrySaveChangesAsync();

        return plainSecret;
    }

    public async Task<M2mClient> GetByClientIdAsync(string clientId)
    {
        return await _context.Set<M2mClient>()
            .Include(c => c.Scopes)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.DeletedAt == null);
    }

    public async Task<M2mClient> GetByIdAsync(long id)
    {
        return await _context.Set<M2mClient>()
            .Include(c => c.Scopes)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
    }

    public async Task<List<M2mClient>> GetAllAsync()
    {
        return await _context.Set<M2mClient>()
            .Include(c => c.Scopes)
            .Include(c => c.CreatedByUser)
            .Where(c => c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task RecordUsageAsync(string clientId, string ipAddress)
    {
        var client = await _context.Set<M2mClient>()
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (client != null)
        {
            client.RecordUsage(ipAddress);
            await _context.TrySaveChangesAsync();
        }
    }

    public async Task UpdateScopesAsync(long clientId, string[] scopes)
    {
        var client = await _context.Set<M2mClient>()
            .Include(c => c.Scopes)
            .FirstOrDefaultAsync(c => c.Id == clientId && c.DeletedAt == null);

        if (client == null)
        {
            throw new InvalidOperationException($"M2M client with ID {clientId} not found");
        }

        var existingScopes = client.Scopes.ToList();
        _context.Set<M2mClientScope>().RemoveRange(existingScopes);

        foreach (var scope in scopes.Where(M2mScope.IsValidScope))
        {
            _context.Set<M2mClientScope>().Add(new M2mClientScope(clientId, scope));
        }

        await _context.TrySaveChangesAsync();
    }

    public async Task DeactivateAsync(long clientId)
    {
        var client = await _context.Set<M2mClient>()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.DeletedAt == null);

        if (client == null)
        {
            throw new InvalidOperationException($"M2M client with ID {clientId} not found");
        }

        client.Deactivate();
        await _context.TrySaveChangesAsync();
    }

    public async Task ActivateAsync(long clientId)
    {
        var client = await _context.Set<M2mClient>()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.DeletedAt == null);

        if (client == null)
        {
            throw new InvalidOperationException($"M2M client with ID {clientId} not found");
        }

        client.Activate();
        await _context.TrySaveChangesAsync();
    }

    public async Task DeleteAsync(long clientId)
    {
        var client = await _context.Set<M2mClient>()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.DeletedAt == null);

        if (client == null)
        {
            throw new InvalidOperationException($"M2M client with ID {clientId} not found");
        }

        client.Delete();
        await _context.TrySaveChangesAsync();
    }

    private static string GenerateSecureSecret()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return $"sk_live_{Convert.ToBase64String(bytes).Replace("+", string.Empty).Replace("/", string.Empty).Replace("=", string.Empty)}";
    }
}
