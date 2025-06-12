using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.CompanyReviews.AddCompanyReview;

public class AddCompanyReviewHandler
    : Infrastructure.Services.Mediator.IRequestHandler<AddCompanyReviewCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;
    private readonly ITelegramAdminNotificationService _notificationService;

    public AddCompanyReviewHandler(
        DatabaseContext context,
        IAuthorization authorization,
        ITelegramAdminNotificationService notificationService)
    {
        _context = context;
        _authorization = authorization;
        _notificationService = notificationService;
    }

    public async Task<Nothing> Handle(
        AddCompanyReviewCommand request,
        CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .Include(x => x.Reviews)
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

        if (company is null)
        {
            throw new NotFoundException("Company not found");
        }

        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var hasReviewed = await _context.CompanyReviews
            .HasRecentReviewAsync(
                company.Id,
                user.Id,
                cancellationToken);

        if (hasReviewed)
        {
            throw new BadRequestException("You have already reviewed this company");
        }

        var review = new CompanyReview(
            request.CultureAndValues,
            request.CodeQuality,
            request.WorkLifeBalance,
            request.Management,
            request.CompensationAndBenefits,
            request.CareerOpportunities,
            request.Pros,
            request.Cons,
            request.IWorkHere,
            request.UserEmployment,
            company,
            user);

        company.AddReview(review);

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAboutNewCompanyReviewAsync(
            review,
            company,
            cancellationToken);

        return Nothing.Value;
    }
}