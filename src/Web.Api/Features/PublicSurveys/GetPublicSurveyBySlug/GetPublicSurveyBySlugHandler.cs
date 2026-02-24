using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.GetPublicSurveyBySlug;

public class GetPublicSurveyBySlugHandler
    : IRequestHandler<string, PublicSurveyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetPublicSurveyBySlugHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<PublicSurveyDto> Handle(
        string slug,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);

        var survey = await _context.PublicSurveys
            .AsNoTracking()
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .Include(s => s.Questions)
            .ThenInclude(q => q.Responses)
            .FirstOrDefaultAsync(
                s => s.Slug == slug,
                cancellationToken);

        if (survey == null)
        {
            throw new NotFoundException("Survey not found.");
        }

        var isAuthor = user != null && survey.AuthorId == user.Id;

        if (!isAuthor)
        {
            if (survey.DeletedAt != null || survey.Status == PublicSurveyStatus.Draft)
            {
                throw new NotFoundException("Survey not found.");
            }
        }

        var hasUserResponded = false;
        if (user != null && survey.Questions.Any())
        {
            hasUserResponded = survey.Questions
                .All(q => q.Responses?.Any(r => r.UserId == user.Id) ?? false);
        }

        return new PublicSurveyDto(survey, isAuthor, hasUserResponded);
    }
}
