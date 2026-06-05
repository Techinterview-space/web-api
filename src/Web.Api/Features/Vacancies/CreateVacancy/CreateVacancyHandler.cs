using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Entities.Vacancies;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Web.Api.Features.Vacancies.Dtos;

namespace Web.Api.Features.Vacancies.CreateVacancy;

public class CreateVacancyHandler
    : IRequestHandler<CreateVacancyBodyRequest, VacancyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public CreateVacancyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<VacancyDto> Handle(
        CreateVacancyBodyRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        Company company = null;
        if (request.CompanyId.HasValue && request.CompanyId.Value != Guid.Empty)
        {
            company = await _context.Companies
                .ByIdOrNullAsync(request.CompanyId.Value, cancellationToken)
                ?? throw NotFoundException.CreateFromEntity<Company>(request.CompanyId.Value);
        }

        var vacancy = new Vacancy(
            request.Title,
            company,
            user,
            request.HrContact,
            request.Description,
            request.Status,
            request.CompanyName,
            request.HideAttachedCompany);

        _context.Vacancies.Add(vacancy);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new VacancyDto(vacancy);
    }
}
