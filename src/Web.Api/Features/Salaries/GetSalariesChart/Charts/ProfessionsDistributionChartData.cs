using System;
using System.Collections.Generic;
using Web.Api.Features.Labels.Models;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record ProfessionsDistributionChartData
{
    public ProfessionDistributionData LocalData { get; init; }

    public ProfessionDistributionData RemoteData { get; init; }

    public ProfessionsDistributionChartData(
        ProfessionDistributionData localData,
        ProfessionDistributionData remoteData)
    {
        LocalData = localData ?? new ProfessionDistributionData();
        RemoteData = remoteData ?? new ProfessionDistributionData();
    }
}

public record ProfessionDistributionData
{
    public List<ProfessionDistributionItem> Items { get; init; } = new ();

    public int OtherCount { get; init; }

    public double OtherPercentage { get; init; }

    public int TotalCount { get; init; }

    public ProfessionDistributionData()
    {
        Items = new List<ProfessionDistributionItem>();
        OtherCount = 0;
        OtherPercentage = 0;
        TotalCount = 0;
    }

    public ProfessionDistributionData(
        List<ProfessionDistributionItem> items,
        int otherCount,
        double otherPercentage,
        int totalCount)
    {
        Items = items ?? new List<ProfessionDistributionItem>();
        OtherCount = otherCount;
        OtherPercentage = otherPercentage;
        TotalCount = totalCount;
    }
}

public record ProfessionDistributionItem
{
    public LabelEntityDto Profession { get; init; }

    public int Count { get; init; }

    public double Percentage { get; init; }

    public ProfessionDistributionItem(
        LabelEntityDto profession,
        int count,
        double percentage)
    {
        Profession = profession ?? throw new ArgumentNullException(nameof(profession));
        Count = count;
        Percentage = percentage;
    }
}