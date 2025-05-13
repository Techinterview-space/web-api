using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompany;

public class GetCompanyHandler : IRequestHandler<GetCompanyQuery, CompanyDto>
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

    public async Task<CompanyDto> Handle(
        GetCompanyQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);

        var company = await GetCompanyAsync(
            user,
            request,
            cancellationToken);

        var viewsCounterShouldBeIncreased = false;

        var userIsAllowedToLeaveReview =
            user == null ||
            company.IsUserAllowedToLeaveReview(user.Id);

        if (user is not null &&
            !user.Has(Role.Admin))
        {
            if (company.DeletedAt != null)
            {
                throw new NotFoundException(
                    "Company by ID was not found");
            }

            viewsCounterShouldBeIncreased = true;
        }
        else if (user is null)
        {
            viewsCounterShouldBeIncreased = true;
        }

        if (viewsCounterShouldBeIncreased)
        {
            company.IncreaseViewsCount();
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        return new CompanyDto(
            company,
            userIsAllowedToLeaveReview);
    }

    private async Task<Company> GetCompanyAsync(
        User user,
        GetCompanyQuery request,
        CancellationToken cancellationToken)
    {
        var userIsAdmin = user != null && user.Has(Role.Admin);

        var query = _context.Companies
            .IncludeWhen(userIsAdmin, x => x.Reviews)
            .IncludeWhen(userIsAdmin, x => x.RatingHistory)
            .IncludeWhen(
                !userIsAdmin,
                x => x.Reviews
                    .Where(r => r.ApprovedAt != null && r.OutdatedAt == null));

        Company company = null;
        var identifierAsGuid = request.GetIdentifierAsGuid();
        if (identifierAsGuid.HasValue)
        {
            company = await query
                .FirstOrDefaultAsync(c => c.Id == identifierAsGuid.Value, cancellationToken);
        }
        else
        {
            company = await query
                .FirstOrDefaultAsync(c => c.Slug == request.Identifier, cancellationToken);
        }

        if (company is null)
        {
            throw new NotFoundException(
                "Company not found");
        }

        return company;
    }
}