using System;
using Domain.Entities.Questions;

namespace TechInterviewer.Features.Surveys.Dtos;

public record SalariesSurveyQuestionDto
{
    public SalariesSurveyQuestionDto()
    {
    }

    public SalariesSurveyQuestionDto(
        SalariesSurveyQuestion question)
    {
        Id = question.Id;
        Title = question.Title;
        Description = question.Description;
    }

    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }
}