using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Companies.SoftDeleteCompany;

public class SoftDeleteCompanyHandler : IRequestHandler<SoftDeleteCompanyCommand, Unit>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public SoftDeleteCompanyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Unit> Handle(
        SoftDeleteCompanyCommand request,
        CancellationToken cancellationToken)
    {
        await _authorization.HasRoleOrFailAsync(Role.Admin, cancellationToken);

        var company = await _context.Companies
            .Where(x => x.DeletedAt == null)
            .FirstOrDefaultAsync(
                x => x.Id == request.CompanyId,
                cancellationToken)
            ?? throw new NotFoundException("Company not found");

        company.Delete();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}