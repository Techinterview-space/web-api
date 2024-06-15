using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Salaries.Models;

public record UserSalaryShowInStatsDecisionMaker
{
    private const int MinimumSalaryValueInKzt = 85_000;
    private const int MaximumSalaryValueInKzt = 10_000_000;
    private const double MaximumSalaryWithPrecisionInKzt = MaximumSalaryValueInKzt * 0.9;

    private const int GapInPercentForMinimalSalary = 40;
    private const int GapInPercentForMaximalSalary = 20;
    private const int MinCountOfSalariesToDoDecision = 5;

    private readonly DatabaseContext _context;
    private readonly CurrentUser _currentUser;
    private readonly double _salaryValue;
    private readonly DeveloperGrade _salaryGrade;
    private readonly CompanyType _company;
    private readonly Profession _professionOrNull;

    public UserSalaryShowInStatsDecisionMaker(
        DatabaseContext context,
        CurrentUser currentUser,
        double salaryValue,
        DeveloperGrade salaryGrade,
        CompanyType company,
        Profession professionOrNull)
    {
        _context = context;
        _salaryValue = salaryValue;
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

        return Decide(salaryValues);
    }

    private bool Decide(
        List<double> salaryValues)
    {
        if (salaryValues.Count < MinCountOfSalariesToDoDecision)
        {
            return true;
        }

        if (_salaryValue < MinimumSalaryValueInKzt)
        {
            return false;
        }

        if (BelowMinimalSalary(salaryValues, out var minimalResult))
        {
            return minimalResult;
        }

        if (_salaryValue >= MaximumSalaryWithPrecisionInKzt)
        {
            return false;
        }

        if (AboveMaximumSalary(salaryValues, out var maximumResult))
        {
            return maximumResult;
        }

        return true;
    }

    private bool BelowMinimalSalary(
        List<double> salaryValues,
        out bool returnResult)
    {
        var minimalSalary = salaryValues.First();
        var minimalSalaryGap = minimalSalary * GapInPercentForMinimalSalary / 100;

        if (_salaryValue <= minimalSalary)
        {
            returnResult = _salaryValue >= minimalSalary - minimalSalaryGap;
            return true;
        }

        returnResult = false;
        return false;
    }

    private bool AboveMaximumSalary(
        List<double> salaryValues,
        out bool returnResult)
    {
        var maximalSalary = salaryValues.Last();
        var maximalSalaryGap = maximalSalary * GapInPercentForMaximalSalary / 100;

        if (_salaryValue >= maximalSalary)
        {
            returnResult = _salaryValue <= maximalSalary + maximalSalaryGap;
            return true;
        }

        returnResult = false;
        return false;
    }
}