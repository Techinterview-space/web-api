﻿using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Web.Api.Features.Surveys.Dtos;
using Web.Api.Features.Surveys.Services;

namespace Web.Api.Features.Surveys.ReplyOnSalariesSurvey;

public class ReplyOnSalariesSurveyHandler
    : Infrastructure.Services.Mediator.IRequestHandler<ReplyOnSalariesSurveyCommand, SalariesSurveyReplyDto>
{
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
        var currentUser = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var surveyUserService = new SalariesSurveyUserService(_context);
        var hasRecentReplies = await surveyUserService.HasFilledSurveyAsync(currentUser, cancellationToken);

        if (hasRecentReplies)
        {
            throw new BadRequestException("You have already replied to this question.");
        }

        var reply = new SalariesSurveyReply(
            request.UsefulnessRating,
            currentUser);

        _context.Add(reply);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new SalariesSurveyReplyDto(reply);
    }
}