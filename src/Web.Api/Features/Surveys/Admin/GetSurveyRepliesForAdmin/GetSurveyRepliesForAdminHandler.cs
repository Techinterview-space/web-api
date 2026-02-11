using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Surveys.Dtos;

namespace Web.Api.Features.Surveys.Admin.GetSurveyRepliesForAdmin;

public class GetSurveyRepliesForAdminHandler
    : IRequestHandler<GetSurveyRepliesForAdminQueryParams, Pageable<SalariesSurveyReplyAdminDto>>
{
    private readonly DatabaseContext _context;

    public GetSurveyRepliesForAdminHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<SalariesSurveyReplyAdminDto>> Handle(
        GetSurveyRepliesForAdminQueryParams request,
        CancellationToken cancellationToken)
    {
        return await _context.SalariesSurveyReplies
            .Include(x => x.CreatedByUser)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(
                x => new SalariesSurveyReplyAdminDto(x),
                request,
                cancellationToken);
    }
}
