using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;

namespace Web.Api.Features.JobPostingMessageSubscriptions.Models;

public record CreateJobPostingMessageSubscriptionBody
{
    [Required]
    public string Name { get; init; }

    [Required]
    [NotDefaultValue]
    public long TelegramChatId { get; init; }

    public List<long> ProfessionIds { get; init; } = new List<long>();
}