using System;
using Domain.Entities.Surveys;
using Domain.Validation.Exceptions;
using TestUtils.Fakes;
using Xunit;

namespace Domain.Tests.Entities.Surveys;

public class PublicSurveyOptionTests
{
    [Fact]
    public void Constructor_ValidParams_CreatesOption()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = new PublicSurveyOptionFake(question, text: "Yes", order: 1);

        Assert.Equal("Yes", option.Text);
        Assert.Equal(1, option.Order);
        Assert.Equal(question.Id, option.QuestionId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptyText_Throws(string text)
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        Assert.Throws<ArgumentNullException>(() =>
            new PublicSurveyOption(text, 0, question));
    }

    [Fact]
    public void Constructor_TextTooLong_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var longText = new string('a', PublicSurveyOption.TextMaxLength + 1);

        Assert.Throws<BadRequestException>(() =>
            new PublicSurveyOptionFake(question, text: longText));
    }

    [Fact]
    public void Constructor_NegativeOrder_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PublicSurveyOptionFake(question, order: -1));
    }

    [Fact]
    public void UpdateText_ValidText_UpdatesText()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = new PublicSurveyOptionFake(question);

        option.UpdateText("Updated option");

        Assert.Equal("Updated option", option.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateText_NullOrEmpty_Throws(string text)
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = new PublicSurveyOptionFake(question);

        Assert.Throws<ArgumentNullException>(() => option.UpdateText(text));
    }

    [Fact]
    public void UpdateText_TooLong_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = new PublicSurveyOptionFake(question);

        Assert.Throws<BadRequestException>(() =>
            option.UpdateText(new string('a', PublicSurveyOption.TextMaxLength + 1)));
    }

    [Fact]
    public void UpdateOrder_ValidOrder_UpdatesOrder()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = new PublicSurveyOptionFake(question);

        option.UpdateOrder(3);

        Assert.Equal(3, option.Order);
    }

    [Fact]
    public void UpdateOrder_NegativeOrder_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        var option = new PublicSurveyOptionFake(question);

        Assert.Throws<ArgumentOutOfRangeException>(() => option.UpdateOrder(-1));
    }
}
