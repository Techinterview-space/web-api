using Domain.ValueObjects.Pagination;
using MediatR;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.Admin.GetApprovedSalaries;

public record GetApprovedSalariesQuery : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;