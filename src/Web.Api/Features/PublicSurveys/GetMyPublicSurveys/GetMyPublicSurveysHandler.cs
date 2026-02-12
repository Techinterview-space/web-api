using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.GetMyPublicSurveys;

public class GetMyPublicSurveysHandler
    : IRequestHandler<GetMyPublicSurveysQuery, List<MySurveyListItemDto>>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetMyPublicSurveysHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<List<MySurveyListItemDto>> Handle(
        GetMyPublicSurveysQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var query = _context.PublicSurveys
            .AsNoTracking()
            .Include(s => s.Questions)
            .ThenInclude(q => q.Responses)
            .Where(s => s.AuthorId == user.Id);

        if (!request.IncludeDeleted)
        {
            query = query.Where(s => s.DeletedAt == null);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        var surveys = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return surveys
            .Select(s => new MySurveyListItemDto(s))
            .ToList();
    }
}
