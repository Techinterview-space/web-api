using System;
using Domain.Entities.Questions;

namespace Web.Api.Features.Surveys.Dtos;

public record SalariesSurveyReplyDto
{
    public SalariesSurveyReplyDto()
    {
    }

    public SalariesSurveyReplyDto(
        SalariesSurveyReply reply)
    {
        Id = reply.Id;
        UsefulnessReply = reply.UsefulnessReply;
        ExpectationReply = reply.ExpectationReply;
        CreatedByUserId = reply.CreatedByUserId;
        CreatedAt = reply.CreatedAt;
    }

    public Guid Id { get; init; }

    public SurveyUsefulnessReplyType UsefulnessReply { get; init; }

    public ExpectationReplyType ExpectationReply { get; init; }

    public long? CreatedByUserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}