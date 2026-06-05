using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Vacancies;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Admin.Vacancies.ChangeVacancyStatus;

public class ChangeVacancyStatusHandler
    : IRequestHandler<ChangeVacancyStatusCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ChangeVacancyStatusHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        ChangeVacancyStatusCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var vacancy = await _context.Vacancies
            .Include(x => x.History)
            .ByIdOrNullAsync(request.VacancyId, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<Vacancy>(request.VacancyId);

        vacancy.ChangeStatusByAdmin(request.Status, user);
        await _context.TrySaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }
}
