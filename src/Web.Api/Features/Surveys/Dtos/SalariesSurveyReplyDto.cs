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
        UsefulnessRating = reply.UsefulnessRating;
        CreatedByUserId = reply.CreatedByUserId;
        CreatedAt = reply.CreatedAt;
    }

    public Guid Id { get; init; }

    public int UsefulnessRating { get; init; }

    public long? CreatedByUserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}