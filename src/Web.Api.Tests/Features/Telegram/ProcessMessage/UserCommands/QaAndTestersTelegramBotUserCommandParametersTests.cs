﻿using System.Collections.Generic;
using Domain.Entities.Salaries;
using Infrastructure.Services.Telegram.UserCommands;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Features.Telegram.ProcessMessage.UserCommands;

public class QaAndTestersTelegramBotUserCommandParametersTests
{
    [Fact]
    public void Ctor_TestersProfessions_Ok()
    {
        var professions = new List<Profession>
        {
            new ProfessionFake(1, "Tester"),
            new ProfessionFake(2, "Quality Assurance (QA)"),
            new ProfessionFake(3, "Автоматизатор (Automation tester)"),
            new ProfessionFake(4, "Frontend developer"),
        };

        var target = new QaAndTestersTelegramBotUserCommandParameters(professions);

        Assert.Equal(3, target.SelectedProfessionIds.Count);
        Assert.Equal(professions[0].Id, target.SelectedProfessionIds[0]);
        Assert.Equal(professions[1].Id, target.SelectedProfessionIds[1]);
        Assert.Equal(professions[2].Id, target.SelectedProfessionIds[2]);
    }
}