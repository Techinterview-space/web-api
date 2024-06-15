using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;

namespace Web.Api.Features.Telegram.ProcessMessage.UserCommands;

public record QaAndTestersTelegramBotUserCommandParameters
    : TelegramBotUserCommandParameters
{
    public const string TesterProfessionTitle = "tester";
    public const string QaProfessionTitle = "qa";
    public const string AutomationTesterProfessionTitle = "automation";

    private static readonly List<string> _professionTitles = new ()
    {
        TesterProfessionTitle,
        QaProfessionTitle,
        AutomationTesterProfessionTitle,
    };

    public QaAndTestersTelegramBotUserCommandParameters(
        List<Profession> professions)
        : base(GetProfessionIds(professions))
    {
    }

    public static bool ShouldIncludeGroup(
        string requestedProfession)
    {
        requestedProfession = requestedProfession.ToLowerInvariant();
        return _professionTitles.Any(x => x == requestedProfession);
    }

    private static List<Profession> GetProfessionIds(
        List<Profession> professions)
    {
        var productProfessions = new List<Profession>();

        foreach (var profession in professions)
        {
            var splitTitle = profession.SplitTitle();
            if (_professionTitles.Any(x => splitTitle.Contains(x)))
            {
                productProfessions.Add(profession);
            }
        }

        return productProfessions;
    }
}