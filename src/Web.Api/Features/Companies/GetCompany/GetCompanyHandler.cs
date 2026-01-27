using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompany;

public class GetCompanyHandler : Infrastructure.Services.Mediator.IRequestHandler<string, GetCompanyResponse>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetCompanyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<GetCompanyResponse> Handle(
        string identifier,
        CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(
            identifier,
            cancellationToken);

        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);
        var userHasAnyReview =
            user is not null &&
            await _context.CompanyReviews
                .HasRecentReviewAsync(
                    company.Id,
                    user.Id,
                    cancellationToken);

        var viewsCounterShouldBeIncreased =
            user is null ||
            !user.Has(Role.Admin);

        var userIsAllowedToLeaveReview =
            user is null ||
            company.IsUserAllowedToLeaveReview(user.Id);

        if (viewsCounterShouldBeIncreased)
        {
            company.IncreaseViewsCount();
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        return new GetCompanyResponse(
            new CompanyDto(company),
            userIsAllowedToLeaveReview,
            userHasAnyReview);
    }

    private async Task<Company> GetCompanyAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        return await _context.Companies
            .Include(x => x.OpenAiAnalysisRecords)
            .Include(x => x.Reviews
                    .Where(r =>
                        r.ApprovedAt != null &&
                        r.OutdatedAt == null)
                    .OrderByDescending(r => r.CreatedAt))
            .Where(x => x.DeletedAt == null)
            .GetCompanyByIdentifierOrNullAsync(
                identifier,
                cancellationToken)
            ?? throw new NotFoundException("Company not found");
    }
}