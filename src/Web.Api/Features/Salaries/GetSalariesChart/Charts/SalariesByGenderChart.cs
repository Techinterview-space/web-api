using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesByGenderChart
{
    private static readonly List<DeveloperGrade> _grades = new ()
    {
        DeveloperGrade.Junior,
        DeveloperGrade.Middle,
        DeveloperGrade.Senior,
        DeveloperGrade.Lead,
    };

    private static readonly List<Gender> _genders = new ()
    {
        Gender.Female,
        Gender.Male,
        Gender.PreferNotToSay,
    };

    public List<string> Labels { get; }

    public List<SalariesByGenderChartGenderItem> DatasetByGender { get; }

    public SalariesByGenderChart(
        List<UserSalaryDto> salaries)
    {
        Labels = _grades
            .Select(x => x.ToString())
            .ToList();

        DatasetByGender = new List<SalariesByGenderChartGenderItem>();

        var salariesWithGender = salaries
            .Where(x =>
                x.Gender.HasValue &&
                x.Grade.HasValue)
            .Select(x => new
            {
                x.Value,
                Gender = x.Gender.Value,
                Grade = x.Grade.Value,
            })
            .ToList();

        foreach (var gender in _genders)
        {
            var medians = new List<double>();
            var averageValues = new List<double>();

            foreach (var grade in _grades)
            {
                var salariesForGenderAndGrade = salariesWithGender
                    .Where(x => x.Gender == gender && x.Grade == grade)
                    .ToList();

                if (salariesForGenderAndGrade.Count == 0)
                {
                    continue;
                }

                medians.Add(salariesForGenderAndGrade.Median(x => x.Value));
                averageValues.Add(salariesForGenderAndGrade.Average(x => x.Value));
            }

            DatasetByGender.Add(new SalariesByGenderChartGenderItem(
                gender: gender,
                medianSalaries: medians,
                averageSalaries: averageValues));
        }
    }

    public record SalariesByGenderChartGenderItem
    {
        public SalariesByGenderChartGenderItem(
            Gender gender,
            List<double> medianSalaries,
            List<double> averageSalaries)
        {
            Gender = gender;
            MedianSalaries = medianSalaries;
            AverageSalaries = averageSalaries;
        }

        public Gender Gender { get; }

        public List<double> MedianSalaries { get; }

        public List<double> AverageSalaries { get; }
    }
}