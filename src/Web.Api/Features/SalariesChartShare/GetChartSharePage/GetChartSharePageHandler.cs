using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Salaries.GetSalariesChart;

namespace Web.Api.Features.SalariesChartShare.GetChartSharePage
{
    public class GetChartSharePageHandler : IRequestHandler<GetChartSharePageQuery, string>
    {
        private readonly IGlobal _global;
        private readonly DatabaseContext _context;
        private readonly IMediator _mediator;

        public GetChartSharePageHandler(
            DatabaseContext context,
            IGlobal global,
            IMediator mediator)
        {
            _context = context;
            _global = global;
            _mediator = mediator;
        }

        public static string SalaryFormat(
            double salary)
        {
            var ci = CultureInfo.InvariantCulture;
            return salary.ToString("N0", ci);
        }

        public async Task<string> Handle(
            GetChartSharePageQuery request,
            CancellationToken cancellationToken)
        {
            var professionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude).ToList();
            var chartResponse = await _mediator.Send(
                new GetSalariesChartQuery
                {
                    Grade = request.Grade,
                    ProfessionsToInclude = professionsToInclude,
                    Cities = request.Cities,
                },
                cancellationToken);

            var frontendLink = new SalariesChartPageLink(_global, request);

            var formattedMedianSalary = SalaryFormat(chartResponse.MedianSalary);
            var ogTitle = $"Медианная зп: {formattedMedianSalary} тг | techinterview.space";
            string description;
            string contentDescription;

            if (request.HasAnyFilter)
            {
                description = contentDescription = "Специалисты ";
                if (professionsToInclude.Count > 0)
                {
                    var professions = await _context.Professions
                        .Where(x => professionsToInclude.Contains(x.Id))
                        .Select(x => x.Title)
                        .ToListAsync(cancellationToken);

                    description += string.Join(", ", professions);
                    contentDescription += $"<span class=\"\">{string.Join(", ", professions)}</span>";
                }

                if (request.Grade.HasValue)
                {
                    description += $" уровня {request.Grade.ToString()}";
                    contentDescription += $" уровня <span class=\"fw-bold\">{request.Grade.ToString()}</span>";
                }

                description += $" зарабатывают в среднем {formattedMedianSalary} тг.";
                contentDescription += " зарабатывают в среднем:";
            }
            else
            {
                contentDescription = "Специалисты IT в Казахстане зарабатывают в среднем:";
                description = $"Специалисты IT в Казахстане зарабатывают в среднем {formattedMedianSalary} тг.";
            }

            description += $" Более подробно на сайте {frontendLink}";

            return $@"<!doctype html>
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
    <meta property=""og:url"" content=""{frontendLink}"" />
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
            <span class=""display-1"">{chartResponse.MedianSalary:n0}</span>
            <span class=""ms-2"">тенге</span>
        </div>
        <div class=""mb-3 fst-italic text-muted"">
            Рассчитано на основе {chartResponse.TotalCountInStats} анкет(ы).
        </div>
        <div class=""mt-3 mb-3 fs-3 fw-light"">
            Подробнее на сайте <a href=""{frontendLink}"">techinterview.space/salaries</a>
        </div>
    </div>
  <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz"" crossorigin=""anonymous""></script>
  </body>
</html>";
        }
    }
}