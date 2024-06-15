using System.Collections.Generic;
using Domain.Entities.Enums;
using Infrastructure.Services.Global;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;

namespace Web.Api.Views;

public record ChartSharePageViewModel
{
    public ChartSharePageViewModel(
        SalariesChartResponse chartResponse,
        SalariesChartQueryParams requestParams,
        IGlobal global,
        List<string> professionsToInclude)
    {
        HasAnyFilter = requestParams.HasAnyFilter;
        ProfessionsToInclude = string.Join(",", professionsToInclude);

        var frontendUrl = global.FrontendBaseUrl + "/salaries";
        string queryParams = null;

        string description;

        if (requestParams.HasAnyFilter)
        {
            description = "Специалисты ";
            if (requestParams.ProfessionsToInclude.Count > 0)
            {
                description += string.Join(", ", professionsToInclude);
                queryParams += $"?profsInclude={string.Join(",", requestParams.ProfessionsToInclude)}";
            }

            if (requestParams.Grade.HasValue)
            {
                description += $" уровня {requestParams.Grade.ToString()}";

                queryParams = queryParams != null ? queryParams + "&" : "?";
                queryParams += $"grade={(int)requestParams.Grade.Value}";
            }

            description += " зарабатывают в среднем " + chartResponse.MedianSalary + " тенге.";
        }
        else
        {
            description = "Специалисты IT в Казахстане зарабатывают в среднем " + chartResponse.MedianSalary + " тенге.";
        }

        Grade = requestParams.Grade;
        FrontendUrlToRedirect = frontendUrl + queryParams;
        MedianSalary = chartResponse.MedianSalary;
        CountOfSalaries = chartResponse.Salaries.Count;
        MetaDescription = description;
    }

    public bool HasAnyFilter { get; init; }

    public string FrontendUrlToRedirect { get; init; }

    public double MedianSalary { get; init; }

    public int CountOfSalaries { get; init; }

    public string MetaDescription { get; init; }

    public DeveloperGrade? Grade { get; init; }

    public string ProfessionsToInclude { get; init; }

    public bool HasProfessionsToInclude => !string.IsNullOrEmpty(ProfessionsToInclude);
}