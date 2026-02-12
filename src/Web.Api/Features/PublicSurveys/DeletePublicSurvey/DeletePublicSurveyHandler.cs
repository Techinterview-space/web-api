using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.PublicSurveys.DeletePublicSurvey;

public class DeletePublicSurveyHandler
    : IRequestHandler<Guid, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public DeletePublicSurveyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        Guid surveyId,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var survey = await _context.PublicSurveys
            .FirstOrDefaultAsync(
                s => s.Id == surveyId && s.AuthorId == user.Id,
                cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

        survey.Delete();

        await _context.TrySaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}
