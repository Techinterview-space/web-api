using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.HistoricalRecords;
using Domain.Enums;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart;

public record GetSalariesHistoricalChartResponse
{
    public GetSalariesHistoricalChartResponse()
    {
    }

    public GetSalariesHistoricalChartResponse(
        List<HistoricalDataByTemplate> templates,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        Templates = templates;
        From = from;
        To = to;
    }

    public List<HistoricalDataByTemplate> Templates { get; private set; } = new ();

    public DateTimeOffset From { get; private set; }

    public DateTimeOffset To { get; private set; }

    public static GetSalariesHistoricalChartResponse NoSalaryOrAuthorization(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        return new GetSalariesHistoricalChartResponse
        {
            From = from,
            To = to,
        };
    }
}

public record HistoricalDataByTemplate
{
    public HistoricalDataByTemplate(
        Guid templateId,
        string name,
        List<long> professionIds,
        List<HistoricalDataPoint> dataPoints)
    {
        TemplateId = templateId;
        Name = name;
        ProfessionIds = professionIds;
        DataPoints = dataPoints;
    }

    public Guid TemplateId { get; }

    public string Name { get; }

    public List<long> ProfessionIds { get; }

    public List<HistoricalDataPoint> DataPoints { get; }
}

public record HistoricalDataPoint
{
    public HistoricalDataPoint(
        SalariesHistoricalDataRecord record)
    {
        Date = record.Date;
        MedianLocalSalary = record.Data.MedianLocalSalary;
        AverageLocalSalary = record.Data.AverageLocalSalary;
        MinLocalSalary = record.Data.MinLocalSalary;
        MaxLocalSalary = record.Data.MaxLocalSalary;
        TotalSalaryCount = record.Data.TotalSalaryCount;
        MedianLocalSalaryByGrade = record.Data.MedianLocalSalaryByGrade ?? new Dictionary<Domain.Enums.GradeGroup, double>();
    }

    public DateTimeOffset Date { get; }

    public double MedianLocalSalary { get; }

    public double AverageLocalSalary { get; }

    public double? MinLocalSalary { get; }

    public double? MaxLocalSalary { get; }

    public int TotalSalaryCount { get; }

    public Dictionary<GradeGroup, double> MedianLocalSalaryByGrade { get; }
}