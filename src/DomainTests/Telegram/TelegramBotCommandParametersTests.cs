using System.Collections.Generic;
using Domain.Entities.Salaries;
using Domain.Telegram;
using Xunit;

namespace DomainTests.Telegram;

public class TelegramBotCommandParametersTests
{
    [Fact]
    public void GetProfessionsTitleOrNull_SeveralProfessions_ReturnsString()
    {
        var target = new TelegramBotCommandParameters(
            new List<Profession>
            {
                new Profession("Frontend Developer"),
                new Profession("Backend Developer"),
                new Profession("QA"),
            });

        Assert.Equal(
            "Frontend Developer, Backend Developer, QA",
            target.GetProfessionsTitleOrNull());
    }

    [Fact]
    public void GetProfessionsTitleOrNull_OneProfession_ReturnsString()
    {
        var target = new TelegramBotCommandParameters(
            new List<Profession>
            {
                new Profession("QA"),
            });

        Assert.Equal(
            "QA",
            target.GetProfessionsTitleOrNull());
    }

    [Fact]
    public void GetProfessionsTitleOrNull_NoProfessions_ReturnsNull()
    {
        var target = new TelegramBotCommandParameters();
        Assert.Null(target.GetProfessionsTitleOrNull());
    }
}