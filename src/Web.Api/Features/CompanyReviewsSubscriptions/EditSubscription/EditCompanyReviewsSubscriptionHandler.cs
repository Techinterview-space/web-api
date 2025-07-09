using System.Threading;
using System.Threading.Tasks;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.CompanyReviewsSubscriptions.GetSalarySubscriptions;

namespace Web.Api.Features.CompanyReviewsSubscriptions.EditSubscription;

public class EditCompanyReviewsSubscriptionHandler
    : Infrastructure.Services.Mediator.IRequestHandler<EditCompanyReviewsSubscriptionCommand, CompanyReviewsSubscriptionDto>
{
    private readonly DatabaseContext _context;

    public EditCompanyReviewsSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<CompanyReviewsSubscriptionDto> Handle(
        EditCompanyReviewsSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        if (string.IsNullOrEmpty(request.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        var existingSubscription = await _context.CompanyReviewsSubscriptions
            .FirstOrDefaultAsync(
                x => x.Id == request.SubscriptionId,
                cancellationToken);

        if (existingSubscription == null)
        {
            throw new NotFoundException("Subscription does not exist.");
        }

        existingSubscription.Update(
            request.Name,
            request.Regularity,
            request.UseAiAnalysis);

        await _context.SaveChangesAsync(cancellationToken);
        return new CompanyReviewsSubscriptionDto(existingSubscription);
    }
}