using System;
using System.Collections.Generic;
using Domain.Entities.Users;
using Domain.Validation;

namespace Domain.Entities.Auth;

public class M2mClient : BaseModel, IHasDeletedAt
{
    protected M2mClient()
    {
    }

    public M2mClient(
        string name,
        string description,
        long createdByUserId)
    {
        Name = name.ThrowIfNullOrEmpty(nameof(name));
        Description = description;
        ClientId = Guid.NewGuid().ToString("N");
        IsActive = true;
        CreatedByUserId = createdByUserId;
        Scopes = new List<M2mClientScope>();
    }

    public string Name { get; protected set; }

    public string Description { get; protected set; }

    public string ClientId { get; protected set; }

    public string ClientSecretHash { get; protected set; }

    public bool IsActive { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<M2mClientScope> Scopes { get; protected set; }

    public long CreatedByUserId { get; protected set; }

    public virtual User CreatedByUser { get; protected set; }

    public DateTimeOffset? LastUsedAt { get; protected set; }

    public string LastUsedIpAddress { get; protected set; }

    public int? RateLimitPerMinute { get; protected set; }

    public int? RateLimitPerDay { get; protected set; }

    public void SetClientSecret(string hashedSecret)
    {
        ClientSecretHash = hashedSecret;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void RecordUsage(string ipAddress)
    {
        LastUsedAt = DateTimeOffset.UtcNow;
        LastUsedIpAddress = ipAddress;
    }

    public void Update(
        string name,
        string description,
        int? rateLimitPerMinute,
        int? rateLimitPerDay)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name;
        }

        Description = description;
        RateLimitPerMinute = rateLimitPerMinute;
        RateLimitPerDay = rateLimitPerDay;
    }

    public void Delete()
    {
        if (DeletedAt.HasValue)
        {
            throw new InvalidOperationException($"M2M Client Id:{Id} is already deleted");
        }

        DeletedAt = DateTimeOffset.UtcNow;
        IsActive = false;
    }
}
