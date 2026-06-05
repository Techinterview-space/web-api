using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Vacancies.Dtos;

namespace Web.Api.Features.Vacancies.SearchVacancies;

public class SearchVacanciesHandler
    : IRequestHandler<SearchVacanciesQueryParams, Pageable<VacancyListItemDto>>
{
    private readonly DatabaseContext _context;

    public SearchVacanciesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<VacancyListItemDto>> Handle(
        SearchVacanciesQueryParams request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.GetPageSize();
        var page = request.Page < 1 ? 1 : request.Page;
        var searchQuery = request.SearchQuery?.Trim().ToLowerInvariant();

        return await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Company)
            .Where(x =>
                x.Status == VacancyStatus.Public &&
                x.DeletedAt == null)
            .When(
                searchQuery != null && searchQuery.Length >= 2,
                x =>
                    x.Title.ToLower().Contains(searchQuery) ||
                    (x.Description != null && x.Description.ToLower().Contains(searchQuery)))
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .AsPaginatedAsync(
                x => new VacancyListItemDto(x),
                new PageModel(
                    page,
                    pageSize),
                cancellationToken);
    }
}
