using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Employments;
using MG.Utils.Abstract.Extensions;
using MG.Utils.Pagination;
using MG.Utils.ValueObjects;

namespace Domain.Services.Organizations.Requests;

public record CandidateCardsFilterPaginatedRequest : PageModel, ICandidateCardsFilter
{
    public string Statuses { get; init; } = string.Empty;

    public bool IncludeEntities { get; init; }

    public ActiveOrArchived ActiveOrArchived { get; init; } = ActiveOrArchived.Active;

    public ICollection<EmploymentStatus> GetStatuses()
    {
        if (string.IsNullOrEmpty(Statuses))
        {
            return EnumHelper.Values<EmploymentStatus>()
                .Where(x => x != EmploymentStatus.Undefined)
                .ToList();
        }

        return new IntArrayParameter(Statuses)
            .Where(x => x != 0)
            .Select(x => (EmploymentStatus)x)
            .ToList()
            .AsReadOnly();
    }
}

public enum ActiveOrArchived
{
    Undefined,
    Active,
    Archived,
    Both,
}