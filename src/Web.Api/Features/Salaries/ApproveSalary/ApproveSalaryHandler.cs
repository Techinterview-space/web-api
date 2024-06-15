using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Salaries.ApproveSalary;

public class ApproveSalaryHandler : IRequestHandler<ApproveSalaryCommand, Unit>
{
    private readonly DatabaseContext _context;

    public ApproveSalaryHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        ApproveSalaryCommand request,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Salary record not found");

        salary.Approve();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}