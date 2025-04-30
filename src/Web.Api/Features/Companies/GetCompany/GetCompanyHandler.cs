using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        var userIsAdmin = user != null && user.Has(Role.Admin);

        var company = await _context.Companies
            .IncludeWhen(userIsAdmin, x => x.Reviews)
            .IncludeWhen(userIsAdmin, x => x.RatingHistory)
            .IncludeWhen(
                !userIsAdmin,
                x => x.Reviews
                    .Where(r => r.ApprovedAt != null && r.OutdatedAt == null))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (company is null)
        {
            throw new NotFoundException(
                "Company not found");
        }

        var userIsAllowedToLeaveReview =
            user == null ||
            company.IsUserAllowedToLeaveReview(user.Id);

        if (user is not null &&
            !user.Has(Role.Admin) &&
            company.DeletedAt != null)
        {
            throw new NotFoundException(
                "Company by ID was not found");
        }

        return new CompanyDto(
            company,
            userIsAllowedToLeaveReview);
    }
}