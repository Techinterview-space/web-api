using Domain.Entities.StatData;
using Domain.Extensions;

namespace Infrastructure.Services.AiServices.Custom.Models;

public record OpenAiBodyReportRoleSalaryData
{
    public OpenAiBodyReportRoleSalaryData(
        List<SalaryBaseData> salaries)
    {
        if (salaries.Count == 0)
        {
            Average = 0;
            Median = 0;
            Min = null;
            Max = null;
            Count = 0;
            return;
        }

        var salaryValues = salaries
            .Select(x => x.Value)
            .ToList();

        Average = salaryValues.Average();
        Median = salaryValues.Median();
        Min = salaryValues.Min();
        Max = salaryValues.Max();
        Count = salaryValues.Count;
    }

    public double Average { get; }

    public double Median { get; }

    public double? Min { get; }

    public double? Max { get; }

    public int Count { get; }
}