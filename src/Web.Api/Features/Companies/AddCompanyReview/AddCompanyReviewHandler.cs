using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Companies.AddCompanyReview;

public class AddCompanyReviewHandler
    : IRequestHandler<AddCompanyReviewCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public AddCompanyReviewHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
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
            .AnyAsync(
                cr =>
                    cr.CompanyId == request.CompanyId &&
                    cr.UserId == user.Id &&
                    cr.OutdatedAt == null,
                cancellationToken: cancellationToken);

        if (hasReviewed)
        {
            throw new BadRequestException("You have already reviewed this company");
        }

        company.AddReview(
            new CompanyReview(
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
                user));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}