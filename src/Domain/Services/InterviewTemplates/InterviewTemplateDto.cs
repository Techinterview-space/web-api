﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Interviews;
using Domain.Services.Labels;
using Domain.Services.Organizations;
using Domain.Services.Users;

namespace Domain.Services.InterviewTemplates;

public record InterviewTemplateDto : InterviewTemplateUpdateRequest
{
    public InterviewTemplateDto()
    {
    }

    public InterviewTemplateDto(
        InterviewTemplate interviewTemplate)
    {
        Id = interviewTemplate.Id;
        Title = interviewTemplate.Title;
        OverallOpinion = interviewTemplate.OverallOpinion;
        AuthorId = interviewTemplate.AuthorId;
        Author = UserDto.CreateFromEntityOrNull(interviewTemplate.Author);
        Subjects = interviewTemplate.Subjects;
        CreatedAt = interviewTemplate.CreatedAt;
        UpdatedAt = interviewTemplate.UpdatedAt;
        IsPublic = interviewTemplate.IsPublic;
        Labels = interviewTemplate.Labels.Select(x => new LabelDto(x)).ToList();
        OrganizationId = interviewTemplate.OrganizationId;
        Organization = OrganizationSimpleDto.CreateFromEntityOrNull(interviewTemplate.Organization);
    }

    public long AuthorId { get; init; }

    public UserDto Author { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public OrganizationSimpleDto Organization { get; init; }
}