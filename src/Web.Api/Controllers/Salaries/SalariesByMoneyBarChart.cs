using System.Collections.Generic;

namespace TechInterviewer.Controllers.Salaries;

public record SalariesByMoneyBarChart
{
    public SalariesByMoneyBarChart()
    {
    }

    public List<string> Labels { get; }
}