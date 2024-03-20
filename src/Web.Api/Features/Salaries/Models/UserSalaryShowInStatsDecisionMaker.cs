using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace TechInterviewer.Features.Salaries.Models;

public record UserSalaryShowInStatsDecisionMaker
{
    private const int GapInPercentForMinimalSalary = 40;
    private const int GapInPercentForMaximalSalary = 20;

    private const int MinCountOfSalariesToDoDecision = 5;

    private readonly DatabaseContext _context;
    private readonly CurrentUser _currentUser;
    private readonly double _salary;
    private readonly DeveloperGrade _salaryGrade;
    private readonly CompanyType _company;
    private readonly Profession _professionOrNull;

    public UserSalaryShowInStatsDecisionMaker(
        DatabaseContext context,
        CurrentUser currentUser,
        double salary,
        DeveloperGrade salaryGrade,
        CompanyType company,
        Profession professionOrNull)
    {
        _context = context;
        _salary = salary;
        _salaryGrade = salaryGrade;
        _company = company;
        _professionOrNull = professionOrNull;
        _currentUser = currentUser;
    }

    public async Task<bool> DecideAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsEmailVerified)
        {
            return false;
        }

        var yearAgoGap = DateTime.UtcNow.AddYears(-1);
        var salaryValues = await _context.Salaries
            .Where(x => x.CreatedAt >= yearAgoGap)
            .Where(x => x.UseInStats)
            .When(_professionOrNull != null, x => x.ProfessionId == _professionOrNull.Id)
            .Where(x =>
                x.Company == _company &&
                x.Grade == _salaryGrade &&
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