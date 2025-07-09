using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData.CompanyReviews;
using Domain.Entities.StatData.Salary;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviewsSubscriptions.DeleteSubscription;

public class DeleteCompanyReviewsSubscriptionHandler
    : IRequestHandler<DeleteCompanyReviewsSubscriptionCommand, Nothing>
{
    private readonly DatabaseContext _context;

    public DeleteCompanyReviewsSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Nothing> Handle(
        DeleteCompanyReviewsSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var cacheRecord = await _context.CompanyReviewsSubscriptions
                              .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                          ?? throw NotFoundException.CreateFromEntity<LastWeekCompanyReviewsSubscription>(request.Id);

        _context.CompanyReviewsSubscriptions.Remove(cacheRecord);

        await _context.SaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }
}