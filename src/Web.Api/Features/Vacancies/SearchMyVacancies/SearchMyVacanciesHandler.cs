using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Vacancies.Dtos;

namespace Web.Api.Features.Vacancies.SearchMyVacancies;

public class SearchMyVacanciesHandler
    : IRequestHandler<SearchMyVacanciesQueryParams, SearchMyVacanciesResponse>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public SearchMyVacanciesHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<SearchMyVacanciesResponse> Handle(
        SearchMyVacanciesQueryParams request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);
        var pageSize = request.GetPageSize();
        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var searchQuery = request.SearchQuery?.Trim().ToLowerInvariant();

        var page = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Company)
            .Where(x => x.AuthorId == user.Id)
            .When(request.Deleted, x => x.DeletedAt != null)
            .When(
                !request.Deleted && request.Status.HasValue,
                x => x.Status == request.Status.Value && x.DeletedAt == null)
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
                    pageNumber,
                    pageSize),
                cancellationToken);

        var hasAnyVacancy = await _context.Vacancies
            .AnyAsync(x => x.AuthorId == user.Id, cancellationToken);

        return new SearchMyVacanciesResponse(
            pageNumber,
            pageSize,
            page.TotalItems,
            page.Results,
            hasAnyVacancy);
    }
}
