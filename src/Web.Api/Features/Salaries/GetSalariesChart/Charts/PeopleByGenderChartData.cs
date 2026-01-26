using System.Collections.Generic;
using Domain.Enums;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record PeopleByGenderChartData
{
    public GenderDistributionData LocalData { get; init; }

    public GenderDistributionData RemoteData { get; init; }

    public PeopleByGenderChartData(
        GenderDistributionData localData,
        GenderDistributionData remoteData)
    {
        LocalData = localData ?? new GenderDistributionData();
        RemoteData = remoteData ?? new GenderDistributionData();
    }
}

public record GenderDistributionData
{
    public List<GenderDistributionItem> Items { get; init; } = new ();

    public int NoGenderCount { get; init; }

    public double NoGenderPercentage { get; init; }

    public int TotalCount { get; init; }

    public GenderDistributionData()
    {
        Items = new List<GenderDistributionItem>();
        NoGenderCount = 0;
        NoGenderPercentage = 0;
        TotalCount = 0;
    }

    public GenderDistributionData(
        List<GenderDistributionItem> items,
        int noGenderCount,
        double noGenderPercentage,
        int totalCount)
    {
        Items = items ?? new List<GenderDistributionItem>();
        NoGenderCount = noGenderCount;
        NoGenderPercentage = noGenderPercentage;
        TotalCount = totalCount;
    }
}

public record GenderDistributionItem
{
    public Gender Gender { get; init; }

    public int Count { get; init; }

    public double Percentage { get; init; }

    public GenderDistributionItem(
        Gender gender,
        int count,
        double percentage)
    {
        Gender = gender;
        Count = count;
        Percentage = percentage;
    }
}