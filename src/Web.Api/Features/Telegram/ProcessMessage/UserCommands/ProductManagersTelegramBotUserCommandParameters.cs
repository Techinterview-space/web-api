using System;
using System.Collections.Generic;
using Domain.Entities.Salaries;

namespace Web.Api.Features.Telegram.ProcessMessage.UserCommands;

public record ProductManagersTelegramBotUserCommandParameters
    : TelegramBotUserCommandParameters
{
    public const string ProductProfessionTitle = "product";

    public ProductManagersTelegramBotUserCommandParameters(
        List<Profession> professions)
        : base(GetProductProfessionIds(professions))
    {
    }

    public static bool ShouldIncludeGroup(
        string requestedProfession) =>
        requestedProfession.Equals(ProductProfessionTitle, StringComparison.InvariantCultureIgnoreCase);

    private static List<Profession> GetProductProfessionIds(
        List<Profession> professions)
    {
        var productProfessions = new List<Profession>();

        foreach (var profession in professions)
        {
            if (profession.SplitTitle().Contains(ProductProfessionTitle))
            {
                productProfessions.Add(profession);
            }
        }

        return productProfessions;
    }
}