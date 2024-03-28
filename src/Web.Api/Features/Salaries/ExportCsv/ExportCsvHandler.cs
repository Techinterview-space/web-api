using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.CSV;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Features.Salaries.ExportCsv;

public class ExportCsvHandler : IRequestHandler<ExportCsvQuery, SalariesCsvResponse>
{
    private const string NoValue = "";

    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public ExportCsvHandler(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    public async Task<SalariesCsvResponse> Handle(
        ExportCsvQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync();
        if (currentUser is null)
        {
            throw new NoPermissionsException("No user found");
        }

        var userCsvDownload = await _context.UserCsvDownloads
            .Where(x => x.UserId == currentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCsvDownload is not null && !userCsvDownload.AllowsDownload())
        {
            throw new NoPermissionsException(
                $"You have downloaded CSV file already withing last {UserCsvDownload.CountOfHoursToAllowDownload} hours.");
        }

        var salaries = await new SalariesForChartQuery(
            _context,
            null,
            null,
            null)
            .ToQueryable()
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Value,Quarter,Year,Age,Gender,Started,Profession,Grade,Company,City,Skill,Industry,Created");

        foreach (var salary in salaries)
        {
            sb.AppendLine(
                $"{salary.Value}," +
                $"{salary.Quarter}," +
                $"{salary.Year}," +
                $"{salary.Age?.ToString() ?? NoValue}," +
                $"{salary.Gender?.ToString() ?? NoValue}," +
                $"{salary.YearOfStartingWork?.ToString() ?? NoValue}," +
                $"{salary.ProfessionId}," +
                $"{salary.Grade?.ToString() ?? NoValue}," +
                $"{salary.Company.ToString()}," +
                $"{salary.City?.ToString() ?? NoValue}," +
                $"{salary.SkillId}," +
                $"{salary.WorkIndustryId}," +
                $"{salary.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? NoValue}");
        }

        if (userCsvDownload is not null)
        {
            _context.UserCsvDownloads.Remove(userCsvDownload);
        }

        _context.UserCsvDownloads.Add(new UserCsvDownload(currentUser));
        await _context.SaveChangesAsync(cancellationToken);

        return new SalariesCsvResponse(sb.ToString());
    }
}