using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Controllers.Salaries;

public record UserSalaryShowInStatsDecisionMaker
{
    private const int GapInPercentForMinimalSalary = 40;
    private const int GapInPercentForMaximalSalary = 20;

    private const int MinCountOfSalariesToDoDecision = 10;

    private readonly DatabaseContext _context;
    private readonly double _salary;
    private readonly DeveloperGrade _salaryGrade;
    private readonly CompanyType _company;
    private readonly UserProfession _profession;

    public UserSalaryShowInStatsDecisionMaker(
        DatabaseContext context,
        double salary,
        DeveloperGrade salaryGrade,
        CompanyType company,
        UserProfession profession)
    {
        _context = context;
        _salary = salary;
        _salaryGrade = salaryGrade;
        _company = company;
        _profession = profession;
    }

    public async Task<bool> DecideAsync(
        CancellationToken cancellationToken)
    {
        var yearAgoGap = DateTime.UtcNow.AddYears(-1);
        var salaryValues = await _context.Salaries
            .Where(x => x.CreatedAt >= yearAgoGap)
            .Where(x =>
                x.Company == _company &&
                x.Grade == _salaryGrade &&
                x.Profession == _profession &&
                x.UseInStats)
            .OrderBy(x => x.Value)
            .Select(x => x.Value)
            .ToListAsync(cancellationToken);

        if (salaryValues.Count < MinCountOfSalariesToDoDecision)
        {
            return true;
        }

        var minimalSalary = salaryValues.First();
        var minimalSalaryGap = minimalSalary * GapInPercentForMinimalSalary / 100;

        if (_salary <= minimalSalary)
        {
            return _salary >= minimalSalary - minimalSalaryGap;
        }

        var maximalSalary = salaryValues.Last();
        var maximalSalaryGap = maximalSalary * GapInPercentForMaximalSalary / 100;

        return _salary <= maximalSalary + maximalSalaryGap;
    }
}