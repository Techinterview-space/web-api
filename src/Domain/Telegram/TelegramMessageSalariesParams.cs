using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Enums;
using Domain.Salaries;

namespace Domain.Telegram;

public class TelegramMessageSalariesParams : ISalariesChartQueryParams
{
    private static readonly List<DeveloperGrade> _grades = new ()
    {
        DeveloperGrade.Junior,
        DeveloperGrade.Middle,
        DeveloperGrade.Senior,
        DeveloperGrade.Lead,
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
            var grade = _grades.FirstOrDefault(x => x.ToString().Equals(part, StringComparison.InvariantCultureIgnoreCase));
            if (grade != default)
            {
                Grade = grade;
                break;
            }
        }
    }
}