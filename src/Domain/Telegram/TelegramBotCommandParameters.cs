using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Salaries;
using Telegram.Bot.Types;

namespace Domain.Telegram;

public record TelegramBotCommandParameters : ISalariesChartQueryParams
{
    private static readonly List<(DeveloperGrade Grade, List<string> PossibleOptions)> _grades = new ()
    {
        (DeveloperGrade.Junior, new List<string> { "джуниоров", "джуниор", "джун", "джуны" }),
        (DeveloperGrade.Middle, new List<string> { "миддлов", "мидлов", "мидл", "миддл", "мид", "миддлы" }),
        (DeveloperGrade.Senior, new List<string> { "сеньоров", "сеньор", "сеньоры", "синьор", "синьоров", "синьоры", "помидор", "помидоры", "помидоров" }),
        (DeveloperGrade.Lead, new List<string> { "лидов", "лид" }),
    };

    private static readonly List<ProfessionsByChatName> _chatProfessions = new ()
    {
        new ("frontend", new List<UserProfessionEnum>
        {
            UserProfessionEnum.Developer,
            UserProfessionEnum.FrontendDeveloper
        }),
        new ("backend", new List<UserProfessionEnum>
        {
            UserProfessionEnum.Developer,
            UserProfessionEnum.BackendDeveloper
        }),
        new ("ios", new List<UserProfessionEnum>
        {
            UserProfessionEnum.Developer,
            UserProfessionEnum.IosDeveloper,
            UserProfessionEnum.MobileDeveloper
        }),
        new ("android", new List<UserProfessionEnum>
        {
            UserProfessionEnum.Developer,
            UserProfessionEnum.AndroidDeveloper,
            UserProfessionEnum.MobileDeveloper
        }),
    };

    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; }

    public List<KazakhstanCity> Cities => new List<KazakhstanCity>(0);

    public TelegramBotCommandParameters(
        DeveloperGrade? grade,
        params UserProfessionEnum[] professionsToInclude)
    {
        Grade = grade;
        ProfessionsToInclude = professionsToInclude
            .Select(x => (long)x)
            .ToList();
    }

    public TelegramBotCommandParameters(
        string chatName,
        DeveloperGrade? grade)
        : this(grade, GetProfessions(chatName).ToArray())
    {
    }

    public TelegramBotCommandParameters(
        Message message)
        : this(
            message.Chat.Title?.ToLowerInvariant() ?? string.Empty,
            GetGrade(message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()))
    {
    }

    public string GetKeyPostfix()
    {
        var grade = Grade?.ToString() ?? "all";
        var professions = ProfessionsToInclude.Count == 0 ? "all" : string.Join("_", ProfessionsToInclude);
        return $"{grade}_{professions}";
    }

    private static DeveloperGrade? GetGrade(
        IReadOnlyCollection<string> messageParts)
    {
        foreach (var part in messageParts)
        {
            var grade = _grades.FirstOrDefault(x =>
                x.Grade.ToString().Equals(part, StringComparison.InvariantCultureIgnoreCase) ||
                x.PossibleOptions.Any(y => y.Equals(part, StringComparison.InvariantCultureIgnoreCase)));

            if (grade != default)
            {
                return grade.Grade;
            }
        }

        return null;
    }

    private static List<UserProfessionEnum> GetProfessions(
        string chatName)
    {
        chatName = chatName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(chatName))
        {
            return new List<UserProfessionEnum>(0);
        }

        var professions = _chatProfessions
            .FirstOrDefault(x => x.ChatName.Contains(chatName, StringComparison.InvariantCultureIgnoreCase));

        if (professions == null)
        {
            return new List<UserProfessionEnum>(0);
        }

        return professions.Professions;
    }

#pragma warning disable SA1313
    private record ProfessionsByChatName(
        string ChatName,
        List<UserProfessionEnum> Professions);
#pragma warning restore SA1313
}