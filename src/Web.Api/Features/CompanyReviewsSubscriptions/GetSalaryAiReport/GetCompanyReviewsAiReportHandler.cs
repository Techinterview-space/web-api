using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.AiServices.Reviews;
using Infrastructure.Services.AiServices.Salaries;
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
        var subscription = await _context
            .CompanyReviewsSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.SubscriptionId,
                cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Subscription {request.SubscriptionId} not found");

        return new CompanyReviewsAiReport();
    }
}