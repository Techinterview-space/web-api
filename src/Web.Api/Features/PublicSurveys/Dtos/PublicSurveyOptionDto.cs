using System;
using Domain.Entities.Surveys;

namespace Web.Api.Features.PublicSurveys.Dtos;

public record PublicSurveyOptionDto
{
    public Guid Id { get; init; }

    public string Text { get; init; }

    public int Order { get; init; }

    public PublicSurveyOptionDto()
    {
    }

    public PublicSurveyOptionDto(PublicSurveyOption option)
    {
        Id = option.Id;
        Text = option.Text;
        Order = option.Order;
    }
}
