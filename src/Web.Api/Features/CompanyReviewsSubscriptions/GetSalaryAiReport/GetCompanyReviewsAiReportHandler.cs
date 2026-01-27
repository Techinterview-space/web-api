using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.AiServices.Reviews;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetSalaryAiReport;

public record GetCompanyReviewsAiReportHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetCompanyReviewsAiReportQuery, CompanyReviewsAiReport>
{
    private readonly DatabaseContext _context;

    public GetCompanyReviewsAiReportHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<CompanyReviewsAiReport> Handle(
        GetCompanyReviewsAiReportQuery request,
        CancellationToken cancellationToken)
    {
        var reviewsForLastWeek = await _context.CompanyReviews
            .Include(x => x.Company)
            .Where(x =>
                x.CreatedAt >= DateTime.UtcNow.AddDays(-7) &&
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .Select(CompanyReviewAiReportItem.Transformation)
            .ToListAsync(cancellationToken);

        return new CompanyReviewsAiReport(reviewsForLastWeek);
    }
}