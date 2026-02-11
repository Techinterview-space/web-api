using System;
using Domain.Entities.Questions;

namespace Web.Api.Features.Surveys.Dtos;

public record SalariesSurveyReplyAdminDto
{
    public SalariesSurveyReplyAdminDto()
    {
    }

    public SalariesSurveyReplyAdminDto(
        SalariesSurveyReply reply)
    {
        Id = reply.Id;
        UsefulnessRating = reply.UsefulnessRating;
        CreatedByUserId = reply.CreatedByUserId;
        CreatedByUserEmail = reply.CreatedByUser?.Email;
        CreatedAt = reply.CreatedAt;
    }

    public Guid Id { get; init; }

    public int UsefulnessRating { get; init; }

    public long? CreatedByUserId { get; init; }

    public string CreatedByUserEmail { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
