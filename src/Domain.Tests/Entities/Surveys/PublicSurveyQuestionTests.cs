using System;
using Domain.Entities.Surveys;
using Domain.Validation.Exceptions;
using TestUtils.Fakes;
using Xunit;

namespace Domain.Tests.Entities.Surveys;

public class PublicSurveyQuestionTests
{
    [Fact]
    public void Constructor_ValidParams_CreatesQuestion()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey, text: "What is your role?", order: 1);

        Assert.Equal("What is your role?", question.Text);
        Assert.Equal(1, question.Order);
        Assert.False(question.AllowMultipleChoices);
        Assert.Equal(survey.Id, question.PublicSurveyId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptyText_Throws(string text)
    {
        var survey = new PublicSurveyFake();

        Assert.Throws<ArgumentNullException>(() =>
            new PublicSurveyQuestion(text, 0, survey));
    }

    [Fact]
    public void Constructor_TextTooLong_Throws()
    {
        var survey = new PublicSurveyFake();
        var longText = new string('a', PublicSurveyQuestion.TextMaxLength + 1);

        Assert.Throws<BadRequestException>(() =>
            new PublicSurveyQuestionFake(survey, text: longText));
    }

    [Fact]
    public void Constructor_NegativeOrder_Throws()
    {
        var survey = new PublicSurveyFake();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PublicSurveyQuestionFake(survey, order: -1));
    }

    [Fact]
    public void AddOption_ValidOption_AddsToList()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        var option = question.AddOption("Option A", 0);

        Assert.Single(question.Options);
        Assert.Equal("Option A", option.Text);
    }

    [Fact]
    public void AddOption_ExceedsMaxOptions_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        for (int i = 0; i < PublicSurveyQuestion.MaxOptions; i++)
        {
            question.AddOption($"Option {i}", i);
        }

        Assert.Throws<BadRequestException>(() =>
            question.AddOption("One too many", PublicSurveyQuestion.MaxOptions));
    }

    [Fact]
    public void RemoveOption_BelowMinOptions_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        question.AddOption("Option A", 0);
        var optionB = question.AddOption("Option B", 1);

        Assert.Throws<BadRequestException>(() =>
            question.RemoveOption(optionB.Id));
    }

    [Fact]
    public void RemoveOption_AboveMinOptions_RemovesOption()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        question.AddOption("Option A", 0);
        question.AddOption("Option B", 1);
        var optionC = question.AddOption("Option C", 2);

        question.RemoveOption(optionC.Id);

        Assert.Equal(2, question.Options.Count);
    }

    [Fact]
    public void RemoveOption_NonExistentOption_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        question.AddOption("Option A", 0);
        question.AddOption("Option B", 1);
        question.AddOption("Option C", 2);

        Assert.Throws<NotFoundException>(() =>
            question.RemoveOption(Guid.NewGuid()));
    }

    [Fact]
    public void HasValidOptions_LessThanMin_ReturnsFalse()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        question.AddOption("Only one", 0);

        Assert.False(question.HasValidOptions());
    }

    [Fact]
    public void HasValidOptions_AtLeastMin_ReturnsTrue()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        question.AddOption("Option A", 0);
        question.AddOption("Option B", 1);

        Assert.True(question.HasValidOptions());
    }

    [Fact]
    public void UpdateText_ValidText_UpdatesText()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        question.UpdateText("Updated question?");

        Assert.Equal("Updated question?", question.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateText_NullOrEmpty_Throws(string text)
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        Assert.Throws<ArgumentNullException>(() => question.UpdateText(text));
    }

    [Fact]
    public void UpdateOrder_ValidOrder_UpdatesOrder()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        question.UpdateOrder(5);

        Assert.Equal(5, question.Order);
    }

    [Fact]
    public void SetAllowMultipleChoices_True_SetsValue()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        question.SetAllowMultipleChoices(true);

        Assert.True(question.AllowMultipleChoices);
    }
}
