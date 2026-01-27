using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.StatData.Salary;
using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class SalaryGradeRangesTests
{
    private static readonly List<SalaryBaseData> _fullSetOfSalaries = new List<SalaryBaseData>
    {
        new ()
        {
            Grade = DeveloperGrade.Junior,
            Value = 100_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Junior,
            Value = 150_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Junior,
            Value = 200_000,
        },

        new ()
        {
            Grade = DeveloperGrade.Middle,
            Value = 220_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Middle,
            Value = 350_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Middle,
            Value = 400_000,
        },

        new ()
        {
            Grade = DeveloperGrade.Senior,
            Value = 520_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Senior,
            Value = 550_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Senior,
            Value = 600_000,
        },

        new ()
        {
            Grade = DeveloperGrade.Lead,
            Value = 700_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Lead,
            Value = 750_000,
        },
        new ()
        {
            Grade = DeveloperGrade.Lead,
            Value = 850_000,
        },
    };

    [Theory]
    [InlineData(90_000, DeveloperGrade.Junior)]
    [InlineData(100_000, DeveloperGrade.Junior)]
    [InlineData(200_000, DeveloperGrade.Junior)]
    [InlineData(219_000, DeveloperGrade.Junior)]
    [InlineData(220_000, DeveloperGrade.Junior)]

    [InlineData(360_000, DeveloperGrade.Middle)]
    [InlineData(400_000, DeveloperGrade.Middle)]

    [InlineData(510_000, DeveloperGrade.Senior)]
    [InlineData(520_000, DeveloperGrade.Senior)]
    [InlineData(560_000, DeveloperGrade.Senior)]
    [InlineData(600_000, DeveloperGrade.Senior)]

    [InlineData(690_000, DeveloperGrade.Lead)]
    [InlineData(700_000, DeveloperGrade.Lead)]
    [InlineData(760_000, DeveloperGrade.Lead)]
    [InlineData(850_000, DeveloperGrade.Lead)]
    [InlineData(1_000_000, DeveloperGrade.Lead)]
    public void InWhatRangeValueIs_ProvidedOneValueSalary_Ok(
        double min,
        DeveloperGrade expectedGrade)
    {
        var target = new SalaryGradeRanges(_fullSetOfSalaries, null);
        var result = target.InWhatRangeValueIs(min, null);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expectedGrade, result[0]);
    }

    [Theory]
    [InlineData(90_000, 120_000, DeveloperGrade.Junior)]
    [InlineData(90_000, 220_000, DeveloperGrade.Junior)]
    [InlineData(219_000, 1_200_000, DeveloperGrade.Junior, DeveloperGrade.Lead)]
    [InlineData(219_000, 1_000_000, DeveloperGrade.Junior, DeveloperGrade.Senior, DeveloperGrade.Lead)]
    public void InWhatRangeValueIs_ProvidedSalaryRange_Ok(
        double min,
        double max,
        params DeveloperGrade[] expectedGrades)
    {
        var target = new SalaryGradeRanges(_fullSetOfSalaries, null);
        var result = target.InWhatRangeValueIs(min, max);

        Assert.NotNull(result);
        Assert.Equal(expectedGrades.Length, result.Count);
        Assert.Equal(expectedGrades, result);
    }
}