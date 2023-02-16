using System;

namespace Domain.Services.InterviewTemplates;

public record InterviewTemplateUpdateRequest : InterviewTemplateCreateRequest
{
    public Guid Id { get; init; }
}