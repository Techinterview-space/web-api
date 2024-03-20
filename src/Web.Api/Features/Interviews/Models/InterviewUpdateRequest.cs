using System;

namespace TechInterviewer.Features.Interviews.Models;

public record InterviewUpdateRequest : InterviewCreateRequest
{
    public Guid Id { get; init; }
}