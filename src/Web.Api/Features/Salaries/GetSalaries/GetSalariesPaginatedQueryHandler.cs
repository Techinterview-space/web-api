﻿using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Web.Api.Features.Salaries.Admin;

namespace Web.Api.Features.Salaries.GetSalaries;

public class GetSalariesPaginatedQueryHandler : Infrastructure.Services.Mediator.IRequestHandler<GetSalariesPaginatedQuery, Pageable<UserSalaryDto>>
{
    private readonly DatabaseContext _context;

    public GetSalariesPaginatedQueryHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public Task<Pageable<UserSalaryDto>> Handle(
        GetSalariesPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        var query = new SalariesAdminQuery(_context)
            .WithSource(request.SalarySourceTypes)
            .ApplyFilters(request)
            .ApplyShowInStats(true)
            .ApplyOrder(GetAllSalariesOrderType.CreatedAtDesc)
            .ToPublicDtoQueryable();

        return query.AsPaginatedAsync(request, cancellationToken);
    }
}