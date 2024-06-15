using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Salaries.DeleteSalary;

public class DeleteSalaryHandler : IRequestHandler<DeleteSalaryCommand, Unit>
{
    private readonly DatabaseContext _context;

    public DeleteSalaryHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        DeleteSalaryCommand request,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Salary record not found");

        _context.Salaries.Remove(salary);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}