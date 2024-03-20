using Domain.ValueObjects.Pagination;
using MediatR;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.Admin.GetExcludedFromStatsSalaries;

public record GetExcludedFromStatsSalariesQuery
    : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;