using System;
using Domain.Entities.Surveys;
using Domain.Validation.Exceptions;
using TestUtils.Fakes;
using Xunit;

namespace Domain.Tests.Entities.Surveys;

public class PublicSurveyResponseTests
{
    [Fact]
    public void Constructor_ValidParams_CreatesResponse()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        var response = new PublicSurveyResponse(question, userId: 1);

        Assert.Equal(question.Id, response.QuestionId);
        Assert.Equal(1, response.UserId);
        Assert.Empty(response.SelectedOptions);
    }

    [Fact]
    public void Constructor_NullQuestion_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PublicSurveyResponse(null, userId: 1));
    }

    [Fact]
    public void Constructor_ZeroUserId_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PublicSurveyResponse(question, userId: 0));
    }

    [Fact]
    public void Constructor_NegativeUserId_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PublicSurveyResponse(question, userId: -1));
    }

    [Fact]
    public void AddSelectedOption_ValidOption_AddsToList()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = question.AddOption("Option A", 0);
        var response = new PublicSurveyResponse(question, userId: 1);

        response.AddSelectedOption(option);

        Assert.Single(response.SelectedOptions);
    }

    [Fact]
    public void AddSelectedOption_NullOption_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var response = new PublicSurveyResponse(question, userId: 1);

        Assert.Throws<ArgumentNullException>(() =>
            response.AddSelectedOption(null));
    }

    [Fact]
    public void AddSelectedOption_OptionFromDifferentQuestion_Throws()
    {
        var survey = new PublicSurveyFake();
        var question1 = new PublicSurveyQuestionFake(survey, order: 0);
        var question2 = new PublicSurveyQuestionFake(survey, order: 1);
        var optionFromQ2 = question2.AddOption("Option from Q2", 0);
        var response = new PublicSurveyResponse(question1, userId: 1);

        Assert.Throws<BadRequestException>(() =>
            response.AddSelectedOption(optionFromQ2));
    }

    [Fact]
    public void AddSelectedOption_DuplicateOption_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = question.AddOption("Option A", 0);
        var response = new PublicSurveyResponse(question, userId: 1);

        response.AddSelectedOption(option);

        Assert.Throws<BadRequestException>(() =>
            response.AddSelectedOption(option));
    }
}
