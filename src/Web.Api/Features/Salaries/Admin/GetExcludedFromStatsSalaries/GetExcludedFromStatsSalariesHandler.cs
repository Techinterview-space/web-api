using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using MediatR;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.Admin.GetExcludedFromStatsSalaries;

public class GetExcludedFromStatsSalariesHandler
    : IRequestHandler<GetExcludedFromStatsSalariesQuery, Pageable<UserSalaryAdminDto>>
{
    private readonly DatabaseContext _context;

    public GetExcludedFromStatsSalariesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<UserSalaryAdminDto>> Handle(
        GetExcludedFromStatsSalariesQuery request,
        CancellationToken cancellationToken)
    {
        var query = new AdminSalariesQuery(_context).ToQueryable(request, false);
        return await query.AsPaginatedAsync(request, cancellationToken);
    }
}