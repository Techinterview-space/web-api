using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Vacancies.Dtos;

namespace Web.Api.Features.Vacancies.GetVacancy;

public class GetVacancyHandler
    : Infrastructure.Services.Mediator.IRequestHandler<Guid, VacancyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetVacancyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<VacancyDto> Handle(
        Guid id,
        CancellationToken cancellationToken)
    {
        var vacancy = await _context.Vacancies
            .AsNoTracking()
            .Include(x => x.Company)
            .ByIdOrNullAsync(id, cancellationToken)
            ?? throw new NotFoundException("Vacancy not found");

        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);
        if (!vacancy.CanBeViewedBy(user))
        {
            throw new NotFoundException("Vacancy not found");
        }

        return new VacancyDto(vacancy);
    }
}
