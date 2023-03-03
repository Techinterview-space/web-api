using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Employments;
using Domain.ValueObjects;

namespace Domain.Services.Organizations.Requests;

public record CandidateCardsFilterRequest : ICandidateCardsFilter
{
    public string Statuses { get; init; } = string.Empty;

    public bool IncludeEntities { get; init; }

    public ActiveOrArchived ActiveOrArchived { get; init; } = ActiveOrArchived.Active;

    public ICollection<EmploymentStatus> GetStatuses()
    {
        return new IntArrayParameter(Statuses)
            .Where(x => x != 0)
            .Select(x => (EmploymentStatus)x)
            .ToList()
            .AsReadOnly();
    }
}