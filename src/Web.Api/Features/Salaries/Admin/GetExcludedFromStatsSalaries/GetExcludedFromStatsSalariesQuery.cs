using Domain.ValueObjects.Pagination;
using MediatR;
using TechInterviewer.Controllers.Salaries;

namespace TechInterviewer.Features.Salaries.Admin.GetExcludedFromStatsSalaries;

public record GetExcludedFromStatsSalariesQuery
    : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;