using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities.StatData.Salary;

public class JobPostingMessageSubscription : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public long TelegramChatId { get; protected set; }

    public List<long> ProfessionIds { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public JobPostingMessageSubscription(
        string name,
        long telegramChatId,
        List<long> professionIds)
    {
        professionIds = professionIds?
            .Distinct()
            .ToList();

        if (professionIds == null || professionIds.Count == 0)
        {
            throw new InvalidOperationException("ProfessionIds cannot be null or empty.");
        }

        Id = Guid.NewGuid();
        Name = name;
        TelegramChatId = telegramChatId;
        ProfessionIds = professionIds;
        DeletedAt = null;
    }

    public void Activate()
    {
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        DeletedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected JobPostingMessageSubscription()
    {
    }
}