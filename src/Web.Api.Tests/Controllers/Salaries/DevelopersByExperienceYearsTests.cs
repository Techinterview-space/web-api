using System;
using System.Collections.Generic;
using Infrastructure.Salaries;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;
using Xunit;

namespace Web.Api.Tests.Controllers.Salaries;

public class DevelopersByExperienceYearsTests
{
    [Fact]
    public void Ctor_ValidData_Ok()
    {
        var currentYear = DateTime.UtcNow.Year;
        var salaries = new List<UserSalaryDto>
        {
            new UserSalaryDto
            {
                Value = 500_000,
                YearOfStartingWork = currentYear - 1,
            },
            new UserSalaryDto
            {
                Value = 500_000,
                YearOfStartingWork = currentYear,
            },
            new UserSalaryDto
            {
                Value = 500_000,
                YearOfStartingWork = currentYear - 4,
            },
            new UserSalaryDto
            {
                Value = 500_000,
                YearOfStartingWork = currentYear - 3,
            },
            new UserSalaryDto
            {
                Value = 500_000,
                YearOfStartingWork = currentYear - 2,
            },
            new UserSalaryDto
            {
                Value = 500_000,
                YearOfStartingWork = currentYear - 18,
            },
        };

        var target = new DevelopersByExperienceYears(
            salaries);

        Assert.Equal(15, target.Labels.Count);
        Assert.Equal(15, target.Data.Count);

        Assert.Equal(1, target.Data[0]);
        Assert.Equal(1, target.Data[1]);
        Assert.Equal(1, target.Data[2]);
        Assert.Equal(1, target.Data[3]);
        Assert.Equal(1, target.Data[4]);

        Assert.Equal(0, target.Data[5]);
        Assert.Equal(0, target.Data[6]);
        Assert.Equal(0, target.Data[7]);
        Assert.Equal(0, target.Data[8]);
        Assert.Equal(0, target.Data[9]);
        Assert.Equal(0, target.Data[10]);
        Assert.Equal(0, target.Data[11]);
        Assert.Equal(0, target.Data[12]);
        Assert.Equal(0, target.Data[13]);
        Assert.Equal(1, target.Data[14]);
    }
}