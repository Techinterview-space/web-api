using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.GetPublicSurveys;

public class GetPublicSurveysHandler
    : IRequestHandler<GetPublicSurveysQuery, Pageable<MySurveyListItemDto>>
{
    private readonly DatabaseContext _context;

    public GetPublicSurveysHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public Task<Pageable<MySurveyListItemDto>> Handle(
        GetPublicSurveysQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PublicSurveys
            .AsNoTracking()
            .Include(s => s.Questions)
            .ThenInclude(q => q.Responses)
            .Where(s => s.DeletedAt == null)
            .Where(s => s.Status == PublicSurveyStatus.Published || s.Status == PublicSurveyStatus.Closed)
            .OrderByDescending(s => s.PublishedAt);

        return query.AsPaginatedAsync(
            s => new MySurveyListItemDto(s),
            request,
            cancellationToken);
    }
}
