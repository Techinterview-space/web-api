using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.PublishPublicSurvey;

public class PublishPublicSurveyHandler
    : IRequestHandler<Guid, PublicSurveyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public PublishPublicSurveyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<PublicSurveyDto> Handle(
        Guid surveyId,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var survey = await _context.PublicSurveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(
                s => s.Id == surveyId && s.AuthorId == user.Id,
                cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

        survey.Publish();

        await _context.TrySaveChangesAsync(cancellationToken);

        return new PublicSurveyDto(survey, isAuthor: true);
    }
}
