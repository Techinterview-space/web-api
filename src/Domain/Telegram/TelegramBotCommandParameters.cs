using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Salaries;

namespace Domain.Telegram;

public record TelegramBotCommandParameters : ISalariesChartQueryParams
{
    public TelegramBotCommandParameters()
        : this(null, new List<long>())
    {
    }

    public TelegramBotCommandParameters(
        string message,
        List<Profession> allProfessions)
        : this(null, DetectProfessions(message, allProfessions))
    {
    }

    public TelegramBotCommandParameters(
        params Profession[] professionsToInclude)
        : this(null, professionsToInclude.Select(x => x.Id).ToList())
    {
    }

    private TelegramBotCommandParameters(
        DeveloperGrade? grade,
        List<long> professionsToInclude)
    {
        Grade = grade;
        ProfessionsToInclude = professionsToInclude;
    }

    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; }

    public List<KazakhstanCity> Cities => new List<KazakhstanCity>(0);

    public string GetKeyPostfix()
    {
        var grade = Grade?.ToString() ?? "all";
        var professions = ProfessionsToInclude.Count == 0 ? "all" : string.Join("_", ProfessionsToInclude);
        return $"{grade}_{professions}";
    }

    private static List<long> DetectProfessions(
        string message,
        List<Profession> allProfessions)
    {
        message = message?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(message) || message.Length < 2)
        {
            return new List<long>(0);
        }

        var professionsToReturn = new List<long>();
        foreach (var profession in allProfessions)
        {
            if (profession.Title.ToLowerInvariant().StartsWith(message))
            {
                professionsToReturn.Add(profession.Id);
            }
        }

        return professionsToReturn;
    }
}