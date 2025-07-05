using System.Collections.Generic;
using Domain.Entities.Enums;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record GradesMinMaxChartData
{
    public List<GradeBoxPlotData> LocalData { get; init; } = new ();

    public List<GradeBoxPlotData> RemoteData { get; init; } = new ();

    public GradesMinMaxChartData(
        List<GradeBoxPlotData> localData,
        List<GradeBoxPlotData> remoteData)
    {
        LocalData = localData ?? new List<GradeBoxPlotData>();
        RemoteData = remoteData ?? new List<GradeBoxPlotData>();
    }
}

public record GradeBoxPlotData
{
    public DeveloperGrade Grade { get; init; }

    public double Min { get; init; }

    public double Q1 { get; init; }

    public double Median { get; init; }

    public double Q3 { get; init; }

    public double Max { get; init; }

    public double Mean { get; init; }

    public List<double> Items { get; init; } = new ();

    public GradeBoxPlotData(
        DeveloperGrade grade,
        double min,
        double q1,
        double median,
        double q3,
        double max,
        double mean,
        List<double> items)
    {
        Grade = grade;
        Min = min;
        Q1 = q1;
        Median = median;
        Q3 = q3;
        Max = max;
        Mean = mean;
        Items = items ?? new List<double>();
    }
}