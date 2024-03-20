using System;

namespace TechInterviewer.Features.Interviews.Models;

public record InterviewTemplateUpdateRequest : InterviewTemplateCreateRequest
{
    public Guid Id { get; init; }
}