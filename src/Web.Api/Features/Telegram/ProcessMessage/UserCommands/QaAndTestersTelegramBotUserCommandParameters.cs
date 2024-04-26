using System;
using System.Collections.Generic;
using Domain.Entities.Salaries;

namespace TechInterviewer.Features.Telegram.ProcessMessage.UserCommands;

public record QaAndTestersTelegramBotUserCommandParameters
    : TelegramBotUserCommandParameters
{
    public const string TesterProfessionTitle = "tester";
    public const string QaProfessionTitle = "qa";
    public const string AutomationTesterProfessionTitle = "automation";

    public QaAndTestersTelegramBotUserCommandParameters(
        List<Profession> professions)
        : base(GetProductProfessionIds(professions))
    {
    }

    public static bool ShouldIncludeGroup(
        string requestedProfession) =>
        requestedProfession.Equals(TesterProfessionTitle, StringComparison.InvariantCultureIgnoreCase) ||
        requestedProfession.Equals(QaProfessionTitle, StringComparison.InvariantCultureIgnoreCase);

    private static List<Profession> GetProductProfessionIds(
        List<Profession> professions)
    {
        var productProfessions = new List<Profession>();

        foreach (var profession in professions)
        {
            if (profession.SplitTitle().Contains(TesterProfessionTitle) ||
                profession.SplitTitle().Contains(QaProfessionTitle) ||
                profession.SplitTitle().Contains(AutomationTesterProfessionTitle))
            {
                productProfessions.Add(profession);
            }
        }

        return productProfessions;
    }
}