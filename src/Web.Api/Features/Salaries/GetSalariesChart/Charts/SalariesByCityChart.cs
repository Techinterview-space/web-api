using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesByCityChart
{
    private static readonly List<KazakhstanCity> _cities = new ()
    {
        KazakhstanCity.Almaty,
        KazakhstanCity.Astana,
        KazakhstanCity.Atyrau,
        KazakhstanCity.Aktau,
        KazakhstanCity.Karaganda,
        KazakhstanCity.Pavlodar,
        KazakhstanCity.Semey,
        KazakhstanCity.Shymkent,
    };

    public List<string> Labels { get; }

    public List<int> ItemsCount { get; }

    public List<double> MedianSalaries { get; }

    public List<double> AverageSalaries { get; }

    public SalariesByCityChart(
        List<UserSalaryDto> salaries)
    {
        var salariesWithCity = salaries
            .Where(x => x.City.HasValue)
            .Select(x => new
            {
                x.Value,
                City = x.City.Value
            })
            .ToList();

        var salariesByCity = new Dictionary<KazakhstanCity, List<double>>();
        var otherSalaries = new List<double>();

        foreach (var salaryWithCity in salariesWithCity)
        {
            if (!_cities.Contains(salaryWithCity.City))
            {
                otherSalaries.Add(salaryWithCity.Value);
                continue;
            }

            if (salariesByCity.TryGetValue(salaryWithCity.City, out var value))
            {
                value.Add(salaryWithCity.Value);
            }
            else
            {
                salariesByCity[salaryWithCity.City] = new List<double> { salaryWithCity.Value };
            }
        }

        Labels = new List<string>();
        ItemsCount = new List<int>();
        MedianSalaries = new List<double>();
        AverageSalaries = new List<double>();

        foreach (var kazakhstanCity in _cities)
        {
            if (salariesByCity.TryGetValue(kazakhstanCity, out var values))
            {
                Labels.Add(kazakhstanCity.ToString());
                ItemsCount.Add(values.Count);
                MedianSalaries.Add(values.Median());
                AverageSalaries.Add(values.Average());
            }
        }

        if (otherSalaries.Count > 0)
        {
            Labels.Add("Other");
            ItemsCount.Add(otherSalaries.Count);
            MedianSalaries.Add(otherSalaries.Median());
            AverageSalaries.Add(otherSalaries.Average());
        }
    }
}