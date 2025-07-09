using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData.CompanyReviews;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.CompanyReviewsSubscriptions.GetSalarySubscriptions;

namespace Web.Api.Features.CompanyReviewsSubscriptions.CreateSubscription;

public class CreateCompanyReviewsSubscriptionHandler
    : Infrastructure.Services.Mediator.IRequestHandler<CreateCompanyReviewsSubscriptionCommand, CompanyReviewsSubscriptionDto>
{
    private readonly DatabaseContext _context;

    public CreateCompanyReviewsSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<CompanyReviewsSubscriptionDto> Handle(
        CreateCompanyReviewsSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (request.TelegramChatId == 0)
        {
            throw new BadRequestException("Telegram chat ID is required.");
        }

        var existingSubscription = await _context.CompanyReviewsSubscriptions
            .FirstOrDefaultAsync(
                x => x.TelegramChatId == request.TelegramChatId,
                cancellationToken);

        if (existingSubscription != null)
        {
            throw new BadRequestException("Subscription already exists.");
        }

        var newSubscription = _context.Add(
            new LastWeekCompanyReviewsSubscription(
                request.Name,
                request.TelegramChatId,
                request.Regularity,
                request.UseAiAnalysis));

        await _context.SaveChangesAsync(cancellationToken);
        return new CompanyReviewsSubscriptionDto(newSubscription.Entity);
    }
}