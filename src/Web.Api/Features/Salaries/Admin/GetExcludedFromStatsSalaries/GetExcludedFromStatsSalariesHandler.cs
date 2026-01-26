using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin.GetExcludedFromStatsSalaries;

public class GetExcludedFromStatsSalariesHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetExcludedFromStatsSalariesQuery, Pageable<UserSalaryAdminDto>>
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
        var query = new SalariesAdminQuery(_context)
            .ApplyFilters(request)
            .ApplyShowInStats(false)
            .ApplyOrder(request.OrderType)
            .ToAdminDtoQueryable();

        return await query.AsPaginatedAsync(request, cancellationToken);
    }
}