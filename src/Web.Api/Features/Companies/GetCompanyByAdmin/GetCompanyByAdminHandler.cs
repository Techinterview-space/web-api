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

namespace Web.Api.Features.Companies.GetCompanyByAdmin;

public class GetCompanyByAdminHandler : IRequestHandler<GetCompanyByAdminQuery, CompanyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetCompanyByAdminHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<CompanyDto> Handle(
        GetCompanyByAdminQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);
        if (!user.Has(Role.Admin))
        {
            throw new NoPermissionsException();
        }

        return new CompanyDto(
            await GetCompanyAsync(
                request,
                cancellationToken));
    }

    private async Task<Company> GetCompanyAsync(
        GetCompanyByAdminQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Companies
            .Include(x => x.Reviews)
            .Include(x => x.RatingHistory)
            .GetCompanyByIdentifierOrNullAsync(
                request.Identifier,
                cancellationToken)
            ?? throw new NotFoundException(
                "Company not found");
    }
}