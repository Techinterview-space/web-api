using System;

namespace Web.Api.Features.Interviews.Models;

public record InterviewTemplateUpdateRequest : InterviewTemplateCreateRequest
{
    public Guid Id { get; init; }
}