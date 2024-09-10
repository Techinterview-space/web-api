using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Salaries;

namespace Web.Api.Features.Telegram.ProcessMessage.UserCommands;

public record TelegramBotUserCommandParameters : ISalariesChartQueryParams
{
    public TelegramBotUserCommandParameters()
        : this(new List<Profession>())
    {
    }

    public TelegramBotUserCommandParameters(
        Profession professionToInclude)
        : this(
            new List<Profession>
            {
                professionToInclude
            })
    {
    }

    public TelegramBotUserCommandParameters(
        List<Profession> professionsToInclude)
    {
        Grade = null;
        SelectedProfessions = professionsToInclude;
        ProfessionsToInclude = professionsToInclude
            .Select(x => x.Id)
            .ToList();
    }

    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; } = new ();

    public List<long> Skills { get; } = new ();

    public List<KazakhstanCity> Cities => new List<KazakhstanCity>(0);

    public List<Profession> SelectedProfessions { get; }

    public SalarySourceType? SalarySourceType { get; init; }

    public int? QuarterTo { get; init; }

    public int? YearTo { get; init; }

    public string GetKeyPostfix()
    {
        var grade = Grade?.ToString() ?? "all";
        var professions = ProfessionsToInclude.Count == 0 ? "all" : string.Join("_", ProfessionsToInclude);
        return $"{grade}_{professions}";
    }

    public string GetProfessionsTitleOrNull()
    {
        if (SelectedProfessions.Count == 0)
        {
            return null;
        }

        return string.Join(", ", SelectedProfessions.Select(x => x.Title));
    }

    public static TelegramBotUserCommandParameters CreateFromMessage(
        string message,
        List<Profession> allProfessions)
    {
        message = message?.Trim().ToLowerInvariant() ?? string.Empty;

        var selectedProfessions = new List<Profession>();

        if (string.IsNullOrEmpty(message) || message.Length < 2)
        {
            return new TelegramBotUserCommandParameters(new List<Profession>(0));
        }

        if (ProductManagersTelegramBotUserCommandParameters.ShouldIncludeGroup(message))
        {
            var productProfessions = ProductManagersTelegramBotUserCommandParameters.GetProductProfessionIds(
                allProfessions);

            return new ProductManagersTelegramBotUserCommandParameters(productProfessions);
        }

        if (QaAndTestersTelegramBotUserCommandParameters.ShouldIncludeGroup(message))
        {
            var testerProfessions = QaAndTestersTelegramBotUserCommandParameters.GetProfessionIds(
                allProfessions);

            return new QaAndTestersTelegramBotUserCommandParameters(testerProfessions);
        }

        foreach (var profession in allProfessions)
        {
            var titleParts = profession.Title
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var titlePart in titleParts)
            {
                if (titlePart.StartsWith(message))
                {
                    selectedProfessions.Add(profession);
                }
            }
        }

        return new TelegramBotUserCommandParameters(selectedProfessions);
    }
}