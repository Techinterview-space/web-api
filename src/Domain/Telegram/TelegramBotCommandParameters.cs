﻿using System;
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
        : this(new List<Profession>())
    {
    }

    public TelegramBotCommandParameters(
        Profession professionToInclude)
        : this(
            new List<Profession>
            {
                professionToInclude
            })
    {
    }

    public TelegramBotCommandParameters(
        List<Profession> professionsToInclude)
    {
        Grade = null;
        SelectedProfessions = professionsToInclude;
        ProfessionsToInclude = professionsToInclude.Select(x => x.Id).ToList();
    }

    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; }

    public List<KazakhstanCity> Cities => new List<KazakhstanCity>(0);

    public List<Profession> SelectedProfessions { get; }

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

    public static TelegramBotCommandParameters CreateFromMessage(
        string message,
        List<Profession> allProfessions)
    {
        message = message?.Trim().ToLowerInvariant() ?? string.Empty;

        var selectedProfessions = new List<Profession>();

        if (string.IsNullOrEmpty(message) || message.Length < 2)
        {
            selectedProfessions = new List<Profession>(0);
        }
        else
        {
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
        }

        return new TelegramBotCommandParameters(selectedProfessions);
    }
}