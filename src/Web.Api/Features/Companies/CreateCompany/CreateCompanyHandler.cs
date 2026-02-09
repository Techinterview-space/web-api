using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.CreateCompany;

public class CreateCompanyHandler : Infrastructure.Services.Mediator.IRequestHandler<CreateCompanyBodyRequest, CompanyDto>
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

        string slug = null;
        if (!string.IsNullOrEmpty(request.Slug))
        {
            slug = new KebabCaseSlug(request.Slug).ToString();
            var hasSameSlug = await _context.Companies
                .AnyAsync(x => x.Slug == slug, cancellationToken);

            if (hasSameSlug)
            {
                throw new BadRequestException("Company with the same slug already exists.");
            }
        }

        var company = new Domain.Entities.Companies.Company(
            request.Name,
            request.Description,
            request.Links,
            request.LogoUrl,
            slug);

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyDto(company);
    }
}