using System.Collections.Generic;
using Domain.Entities.Salaries;

namespace TechInterviewer.Features.Telegram.ProcessMessage.UserCommands;

public record ProductManagersTelegramBotUserCommandParameters
    : TelegramBotUserCommandParameters
{
    public const string ProductProfessionTitle = "product";

    public ProductManagersTelegramBotUserCommandParameters(
        List<Profession> professions)
        : base(GetProductProfessionIds(professions))
    {
    }

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