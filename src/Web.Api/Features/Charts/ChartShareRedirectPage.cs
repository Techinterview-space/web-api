using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Services.Global;
using Microsoft.AspNetCore.Http;
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

    public async Task<ContentResult> CreateAsync(
        HttpResponse response,
        string queryStringOrNull,
        CancellationToken cancellationToken)
    {
        const string title = "Зарплаты в IT в Казахстане";

        var frontendUrl = _global.FrontendBaseUrl + "/salaries" + queryStringOrNull;
        response.Redirect(frontendUrl);
        string description;

        if (_requestParams.HasAnyFilter)
        {
            description = "Специалисты ";
            if (_requestParams.ProfessionsToInclude.Count > 0)
            {
                var professions = await _context.Professions
                    .Where(x => _requestParams.ProfessionsToInclude.Contains(x.Id))
                    .Select(x => x.Title)
                    .ToListAsync(cancellationToken);

                description += string.Join(", ", professions);
            }

            if (_requestParams.Grade.HasValue)
            {
                description += $" уровня {_requestParams.Grade.ToString()}";
            }

            description += " зарабатывают в среднем " + _chartResponse.MedianSalary + " тенге.";
        }
        else
        {
            description = "Специалисты IT в Казахстане зарабатывают в среднем " + _chartResponse.MedianSalary + " тенге.";
        }

        description += $" Более подробно на сайте {frontendUrl}";

        return new ContentResult
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.Redirect,
            Content = $@"<!doctype html>
<html lang=""en"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, shrink-to-fit=no"">
    <meta name=""description"" content=""{description}"" />
    <meta name=""keywords"" content=""зарплата, казахстан, it, salary"" />
    <meta name=""copyright"" content=""Maxim Gorbatyuk"" />

    <meta property=""og:title"" content=""{title}"" />
    <meta property=""og:description"" content=""{description}"" />
    <meta property=""og:type"" content=""website"" />
    <meta property=""og:url"" content=""{frontendUrl}"" />
    <meta property=""og:image"" content=""https://techinterview.space/assets/img/ti_logo_mini.png"" />
    <meta property=""og:site_name"" content=""techinterview.space"" />

    <meta name=""twitter:card"" content=""summary_large_image"" />
    <meta name=""twitter:image"" content=""https://techinterview.space/assets/img/ti_logo_mini.png"" />
    <meta name=""twitter:title"" content=""{title}"" />
    <meta name=""twitter:description"" content=""{description}"" />
    <meta name=""twitter:site"" content=""@techinterview"" />
    <meta name=""twitter:creator"" content=""@maximgorbatyuk"" />

    <!-- Bootstrap CSS -->
    <link rel=""stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css"" integrity=""sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO"" crossorigin=""anonymous"">

    <title>{title}</title>
  </head>
  <body>
  </body>
</html>"
        };
    }
}