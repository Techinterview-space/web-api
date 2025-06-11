using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Companies.UpdateCompany;

public record UpdateCompanyHandler : Infrastructure.Services.Mediator.IRequestHandler<UpdateCompanyCommand, Unit>
{
    private readonly DatabaseContext _context;

    public UpdateCompanyHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        UpdateCompanyCommand request,
        CancellationToken cancellationToken)
    {
        request.Body.ThrowIfInvalid();

        var company = await _context.Companies.FirstOrDefaultAsync(
            c => c.Id == request.CompanyId,
            cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<Company>(request.CompanyId);

        company.Update(
            request.Body.Name,
            request.Body.Description,
            request.Body.LogoUrl,
            request.Body.Links,
            request.Body.Slug);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}