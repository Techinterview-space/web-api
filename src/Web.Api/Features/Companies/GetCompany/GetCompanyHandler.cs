using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompany;

public class GetCompanyHandler : IRequestHandler<GetCompanyQuery, GetCompanyResponse>
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
        GetCompanyQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);

        var company = await GetCompanyAsync(
            request,
            cancellationToken);

        var viewsCounterShouldBeIncreased =
            user is null ||
            !user.Has(Role.Admin);

        var userIsAllowedToLeaveReview =
            user is null ||
            company.IsUserAllowedToLeaveReview(user.Id);

        var userHasAnyReview =
            user is not null &&
            await _context.CompanyReviews
                .Where(r =>
                    r.OutdatedAt == null &&
                    r.UserId == user.Id)
                .AnyAsync(cancellationToken: cancellationToken);

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
        GetCompanyQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Companies
            .Include(x => x.Reviews
                .Where(r => r.ApprovedAt != null && r.OutdatedAt == null))
            .Where(x => x.DeletedAt == null)
            .GetCompanyByIdentifierOrNullAsync(
                request.Identifier,
                cancellationToken)
            ?? throw new NotFoundException("Company not found");
    }
}