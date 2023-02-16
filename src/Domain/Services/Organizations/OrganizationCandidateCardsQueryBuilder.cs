using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Services.Organizations.Requests;
using MG.Utils.EFCore;
using MG.Utils.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Organizations;

public class OrganizationCandidateCardsQueryBuilder
{
    private readonly DatabaseContext _context;
    private readonly Guid _organizationId;
    private readonly ICandidateCardsFilter _request;

    public OrganizationCandidateCardsQueryBuilder(
        DatabaseContext context,
        Guid organizationId,
        ICandidateCardsFilter request)
    {
        _context = context;
        _organizationId = organizationId;
        _request = request;
    }

    public IQueryable<CandidateCard> AsQuery()
    {
        IQueryable<CandidateCard> query = _context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.OpenBy)
            .Include(x => x.Labels);

        if (_request.IncludeEntities)
        {
            query = query
                .Include(x => x.Comments);
        }

        switch (_request.ActiveOrArchived)
        {
            case ActiveOrArchived.Active:
                query = query.Where(x => x.DeletedAt == null);
                break;

            case ActiveOrArchived.Archived:
                query = query.Where(x => x.DeletedAt != null);
                break;
        }

        var statuses = _request.GetStatuses();
        return query
            .Where(x => x.OrganizationId == _organizationId)
            .Where(x => statuses.Contains(x.EmploymentStatus));
    }

    public Task<IEnumerable<CandidateCardDto>> AsArrayDtoAsync()
        => AsQuery().AllAsync(x => new CandidateCardDto(x, true));

    public Task<Pageable<CandidateCardDto>> AsPaginatedDtoAsync(PageModel pagination)
        => AsQuery()
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(x => new CandidateCardDto(x, true), pagination);
}