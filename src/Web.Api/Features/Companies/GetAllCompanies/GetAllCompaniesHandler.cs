using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetAllCompanies;

public class GetAllCompaniesHandler
    : IRequestHandler<GetAllCompaniesQuery, IReadOnlyCollection<CompanyListItemDto>>
{
    private readonly DatabaseContext _context;

    public GetAllCompaniesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CompanyListItemDto>> Handle(
        GetAllCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.ReviewsCount)
            .ThenByDescending(x => x.ViewsCount)
            .ThenByDescending(x => x.Name)
            .Select(x => new CompanyListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Links = x.Links,
                LogoUrl = x.LogoUrl,
                Rating = x.Rating,
                ReviewsCount = x.ReviewsCount,
                ViewsCount = x.ViewsCount,
                Slug = x.Slug,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
