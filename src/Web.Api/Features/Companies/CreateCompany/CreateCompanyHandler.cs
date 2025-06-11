using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.CreateCompany;

public class CreateCompanyHandler : IRequestHandler<CreateCompanyBodyRequest, CompanyDto>
{
    private readonly DatabaseContext _context;

    public CreateCompanyHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> Handle(
        CreateCompanyBodyRequest request,
        CancellationToken cancellationToken)
    {
        var nameUpper = request.Name.ToUpperInvariant();
        var hasSameName = await _context.Companies
            .AnyAsync(x => x.NormalizedName == nameUpper, cancellationToken);

        if (hasSameName)
        {
            throw new BadRequestException("Company with the same name already exists.");
        }

        var company = new Domain.Entities.Companies.Company(
            request.Name,
            request.Description,
            request.Links,
            request.LogoUrl);

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyDto(company);
    }
}