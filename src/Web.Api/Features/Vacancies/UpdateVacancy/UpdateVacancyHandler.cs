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
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Vacancies.UpdateVacancy;

public class UpdateVacancyHandler
    : IRequestHandler<UpdateVacancyCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public UpdateVacancyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        UpdateVacancyCommand request,
        CancellationToken cancellationToken)
    {
        request.Body.ThrowIfInvalid();

        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var vacancy = await _context.Vacancies
            .Include(x => x.History)
            .Include(x => x.Company)
            .ByIdOrNullAsync(request.VacancyId, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<Vacancy>(request.VacancyId);

        vacancy.CouldBeEditedByOrFail(user);

        Company company = null;
        if (request.Body.CompanyId.HasValue && request.Body.CompanyId.Value != Guid.Empty)
        {
            company = await _context.Companies
                .ByIdOrNullAsync(request.Body.CompanyId.Value, cancellationToken)
                ?? throw NotFoundException.CreateFromEntity<Company>(request.Body.CompanyId.Value);
        }

        vacancy.Update(
            request.Body.Title,
            company,
            request.Body.HrContact,
            request.Body.Description,
            request.Body.Status,
            request.Body.CompanyName,
            request.Body.HideAttachedCompany,
            user);

        await _context.TrySaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }
}
