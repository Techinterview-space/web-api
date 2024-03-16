using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Enums;
using Domain.Salaries;

namespace Domain.Telegram;

public class TelegramMessageSalariesParams : ISalariesChartQueryParams
{
    private static readonly List<(DeveloperGrade Grade, List<string> PossibleOptions)> _grades = new ()
    {
        (DeveloperGrade.Junior, new List<string> { "джуниоров", "джуниор", "джун", "джуны" }),
        (DeveloperGrade.Middle, new List<string> { "миддлов", "мидлов", "мидл", "миддл", "мид", "миддлы" }),
        (DeveloperGrade.Senior, new List<string> { "сеньоров", "сеньор", "сеньоры", "синьор", "синьоров", "синьоры", "помидор", "помидоры", "помидоров" }),
        (DeveloperGrade.Lead, new List<string> { "лидов", "лид" }),
    };

    public DeveloperGrade? Grade { get; } = null;

    public List<long> ProfessionsToInclude { get; } = new ();

    public List<KazakhstanCity> Cities { get; } = new ();

    public TelegramMessageSalariesParams(
        string message)
        : this(message?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
    {
    }

    public TelegramMessageSalariesParams(
        string[] messageParts)
    {
        if (messageParts.Length == 0)
        {
            return;
        }

        foreach (var part in messageParts)
        {
            var grade = _grades.FirstOrDefault(x =>
                x.Grade.ToString().Equals(part, StringComparison.InvariantCultureIgnoreCase) ||
                x.PossibleOptions.Any(y => y.Equals(part, StringComparison.InvariantCultureIgnoreCase)));

            if (grade != default)
            {
                Grade = grade.Grade;
                break;
            }
        }
    }

    public TelegramMessageSalariesParams(
        DeveloperGrade grade)
    {
        Grade = grade == default ? null : grade;
    }
}