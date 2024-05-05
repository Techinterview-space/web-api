using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Surveys.Dtos;

namespace TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;

public class ReplyOnSalariesSurveyHandler
    : IRequestHandler<ReplyOnSalariesSurveyCommand, SalariesSurveyReplyDto>
{
    public const int RecentRepliesDays = 180;

    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ReplyOnSalariesSurveyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<SalariesSurveyReplyDto> Handle(
        ReplyOnSalariesSurveyCommand request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();

        var currentUser = await _authorization.CurrentUserOrFailAsync(cancellationToken);

        var createdAtEdge = DateTime.UtcNow.AddDays(-RecentRepliesDays);
        var hasRecentReplies = await _context.SalariesSurveyReplies
            .Where(x =>
                x.CreatedByUserId == currentUser.Id &&
                x.CreatedAt >= createdAtEdge)
            .AnyAsync(cancellationToken);

        if (hasRecentReplies)
        {
            throw new BadRequestException("You have already replied to this question.");
        }

        var reply = new SalariesSurveyReply(
            request.UsefulnessReply,
            request.ExpectationReply,
            currentUser);

        _context.Add(reply);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new SalariesSurveyReplyDto(reply);
    }
}