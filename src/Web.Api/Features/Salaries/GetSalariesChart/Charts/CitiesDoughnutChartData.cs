﻿using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record CitiesDoughnutChartData
{
    public List<CitiesDoughnutChartDataItem> Items { get; }

    public int NoDataCount { get; }

    public CitiesDoughnutChartData(List<UserSalaryDto> salaries)
    {
        var salariesWithCities = salaries
            .Where(x => x.City.HasValue && x.City.Value != KazakhstanCity.Undefined)
            .ToList();

        var cityGroups = salariesWithCities
            .GroupBy(x => x.City.GetValueOrDefault())
            .ToList();

        Items = cityGroups
            .Select(group => new CitiesDoughnutChartDataItem(
                city: group.Key,
                count: group.Count()))
            .OrderByDescending(item => item.Count)
            .ToList();

        NoDataCount = salaries.Count - salariesWithCities.Count;
    }

    public record CitiesDoughnutChartDataItem
    {
        public CitiesDoughnutChartDataItem(
            KazakhstanCity city,
            int count)
        {
            City = city;
            Count = count;
        }

        public KazakhstanCity City { get; }

        public int Count { get; }
    }
}