using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Services.Global;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Salaries.Charts;

namespace TechInterviewer.Features.Charts;

public class ChartShareRedirectPageContentResultHandler
{
    private readonly SalariesChartResponse _chartResponse;
    private readonly SalariesChartQueryParams _requestParams;
    private readonly IGlobal _global;
    private readonly DatabaseContext _context;

    public ChartShareRedirectPageContentResultHandler(
        SalariesChartResponse chartResponse,
        SalariesChartQueryParams requestParams,
        DatabaseContext context,
        IGlobal global)
    {
        _chartResponse = chartResponse;
        _requestParams = requestParams;
        _context = context;
        _global = global;
    }

    public static string SalaryFormat(
        double salary) => salary.ToString("n0");

    public async Task<ContentResult> CreateAsync(
        CancellationToken cancellationToken)
    {
        var frontendUrl = _global.FrontendBaseUrl + "/salaries";
        string queryParams = null;

        var formattedMedianSalary = SalaryFormat(_chartResponse.MedianSalary);
        var ogTitle = $"Медианная зп: {formattedMedianSalary} тг | techinterview.space";
        string description;
        string contentDescription;

        if (_requestParams.HasAnyFilter)
        {
            description = contentDescription = "Специалисты ";
            if (_requestParams.ProfessionsToInclude.Count > 0)
            {
                var professions = await _context.Professions
                    .Where(x => _requestParams.ProfessionsToInclude.Contains(x.Id))
                    .Select(x => x.Title)
                    .ToListAsync(cancellationToken);

                description += string.Join(", ", professions);
                contentDescription += $"<span class=\"\">{string.Join(", ", professions)}</span>";

                queryParams += $"?profsInclude={string.Join(",", _requestParams.ProfessionsToInclude)}";
            }

            if (_requestParams.Grade.HasValue)
            {
                description += $" уровня {_requestParams.Grade.ToString()}";
                contentDescription += $" уровня <span class=\"fw-bold\">{_requestParams.Grade.ToString()}</span>";

                queryParams = queryParams != null ? queryParams + "&" : "?";
                queryParams += $"grade={(int)_requestParams.Grade.Value}";
            }

            description += $" зарабатывают в среднем {formattedMedianSalary} тг.";
            contentDescription += " зарабатывают в среднем:";
        }
        else
        {
            contentDescription = "Специалисты IT в Казахстане зарабатывают в среднем:";
            description = $"Специалисты IT в Казахстане зарабатывают в среднем {formattedMedianSalary} тг.";
        }

        frontendUrl += queryParams;
        description += $" Более подробно на сайте {frontendUrl}";

        return new ContentResult
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
            Content = $@"<!doctype html>
<html lang=""en"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, shrink-to-fit=no"">
    <meta name=""description"" content=""{description}"" />
    <meta name=""keywords"" content=""зарплата, казахстан, it, salary"" />
    <meta name=""copyright"" content=""Maxim Gorbatyuk"" />

    <meta property=""og:title"" content=""{ogTitle}"" />
    <meta property=""og:description"" content=""{description}"" />
    <meta property=""og:type"" content=""website"" />
    <meta property=""og:url"" content=""{frontendUrl}"" />
    <meta property=""og:image"" content=""https://techinterview.space/assets/img/ti_logo_mini.png"" />
    <meta property=""og:site_name"" content=""techinterview.space"" />

    <meta name=""twitter:card"" content=""summary_large_image"" />
    <meta name=""twitter:image"" content=""https://techinterview.space/assets/img/ti_logo_mini.png"" />
    <meta name=""twitter:title"" content=""{ogTitle}"" />
    <meta name=""twitter:description"" content=""{description}"" />
    <meta name=""twitter:site"" content=""@techinterview"" />
    <meta name=""twitter:creator"" content=""@maximgorbatyuk"" />

    <!-- Bootstrap CSS -->
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH"" crossorigin=""anonymous"">

    <title>{ogTitle}</title>
  </head>
  <body>
    <div class=""container"">
        <div class=""mt-5 mb-5 display-3"">Зарплаты в IT в Казахстане</div>
        <div class=""mb-3 fs-3 fw-light"">{contentDescription}</div>
        <div class=""mb-2"">
            <span class=""display-1"">{_chartResponse.MedianSalary:n0}</span>
            <span class=""ms-2"">тенге</span>
        </div>
        <div class=""mb-3 fst-italic text-muted"">
            Рассчитано на основе {_chartResponse.TotalCountInStats} анкет(ы).
        </div>
        <div class=""mt-3 mb-3 fs-3 fw-light"">
            Подробнее на сайте <a href=""{frontendUrl}"">techinterview.space/salaries</a>
        </div>
    </div>
  <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz"" crossorigin=""anonymous""></script>
  </body>
</html>"
        };
    }
}