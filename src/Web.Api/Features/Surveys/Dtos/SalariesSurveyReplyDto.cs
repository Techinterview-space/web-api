using System;
using Domain.Entities.Questions;

namespace TechInterviewer.Features.Surveys.Dtos;

public record SalariesSurveyReplyDto
{
    public SalariesSurveyReplyDto()
    {
    }

    public SalariesSurveyReplyDto(
        SalariesSurveyReply reply)
    {
        Id = reply.Id;
        ReplyType = reply.ReplyType;
        SalariesSurveyQuestionId = reply.SalariesSurveyQuestionId;
        CreatedByUserId = reply.CreatedByUserId;
        CreatedAt = reply.CreatedAt;
    }

    public Guid Id { get; init; }

    public SalariesSurveyReplyType ReplyType { get; init; }

    public Guid SalariesSurveyQuestionId { get; init; }

    public long? CreatedByUserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}