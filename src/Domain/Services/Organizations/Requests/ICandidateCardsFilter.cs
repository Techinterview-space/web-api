using System.Collections.Generic;
using Domain.Entities.Employments;

namespace Domain.Services.Organizations.Requests;

public interface ICandidateCardsFilter
{
    public string Statuses { get; init; }

    public bool IncludeEntities { get; init; }

    public ActiveOrArchived ActiveOrArchived { get; init; }

    public ICollection<EmploymentStatus> GetStatuses();
}