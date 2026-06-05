using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Vacancies;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Vacancies.DeleteVacancy;

public class DeleteVacancyHandler
    : IRequestHandler<DeleteVacancyCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeleteVacancyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        DeleteVacancyCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var vacancy = await _context.Vacancies
            .Include(x => x.History)
            .ByIdOrNullAsync(request.VacancyId, cancellationToken)
            ?? throw NotFoundException.CreateFromEntity<Vacancy>(request.VacancyId);

        if (!vacancy.IsOwnedBy(user) && !user.Has(Role.Admin))
        {
            throw new NoPermissionsException("You are not allowed to delete this vacancy.");
        }

        vacancy.Delete(user);
        await _context.TrySaveChangesAsync(cancellationToken);
        return Nothing.Value;
    }
}
