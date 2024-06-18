using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Infrastructure.Salaries;
using Web.Api.Features.Historical;
using Web.Api.Features.Historical.GetSalariesHistoricalChart;
using Web.Api.Features.Historical.GetSalariesHistoricalChart.Charts;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.GetSalariesHistoricalChart.Charts;

public class SalariesCountWeekByWeekChartTests
{
    private static readonly List<UserSalarySimpleDto> _localSalaries = new List<UserSalarySimpleDto>
    {
        new UserSalarySimpleDto(
            500_000,
            CompanyType.Local,
            DeveloperGrade.Junior,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            300_000,
            CompanyType.Local,
            DeveloperGrade.Junior,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            600_000,
            CompanyType.Local,
            DeveloperGrade.Junior,
            DateTimeOffset.Now.AddDays(-5)),

        new UserSalarySimpleDto(
            700_000,
            CompanyType.Local,
            DeveloperGrade.Middle,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            800_000,
            CompanyType.Local,
            DeveloperGrade.Middle,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            900_000,
            CompanyType.Local,
            DeveloperGrade.Middle,
            DateTimeOffset.Now.AddDays(-5)),

        new UserSalarySimpleDto(
            900_000,
            CompanyType.Local,
            DeveloperGrade.Senior,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            1_000_000,
            CompanyType.Local,
            DeveloperGrade.Senior,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            1_100_000,
            CompanyType.Local,
            DeveloperGrade.Senior,
            DateTimeOffset.Now.AddDays(-5)),
    };

    private static readonly List<UserSalarySimpleDto> _remoteSalaries = new List<UserSalarySimpleDto>
    {
        new UserSalarySimpleDto(
            1_500_000,
            CompanyType.Foreign,
            DeveloperGrade.Junior,
            DateTimeOffset.Now.AddDays(-45)),
        new UserSalarySimpleDto(
            1_300_000,
            CompanyType.Foreign,
            DeveloperGrade.Junior,
            DateTimeOffset.Now.AddDays(-35)),
        new UserSalarySimpleDto(
            1_600_000,
            CompanyType.Foreign,
            DeveloperGrade.Junior,
            DateTimeOffset.Now.AddDays(-15)),

        new UserSalarySimpleDto(
            1_700_000,
            CompanyType.Local,
            DeveloperGrade.Middle,
            DateTimeOffset.Now.AddDays(-5)),
        new UserSalarySimpleDto(
            1_800_000,
            CompanyType.Foreign,
            DeveloperGrade.Middle,
            DateTimeOffset.Now.AddDays(-25)),
        new UserSalarySimpleDto(
            1_900_000,
            CompanyType.Foreign,
            DeveloperGrade.Middle,
            DateTimeOffset.Now.AddDays(-45)),

        new UserSalarySimpleDto(
            1_900_000,
            CompanyType.Foreign,
            DeveloperGrade.Senior,
            DateTimeOffset.Now.AddDays(-45)),
        new UserSalarySimpleDto(
            2_000_000,
            CompanyType.Foreign,
            DeveloperGrade.Senior,
            DateTimeOffset.Now.AddDays(-55)),
        new UserSalarySimpleDto(
            2_100_000,
            CompanyType.Foreign,
            DeveloperGrade.Senior,
            DateTimeOffset.Now.AddDays(-15)),
    };

    [Fact]
    public void Ctor_NoSalaries_Ok()
    {
        var localSalaries = new List<UserSalarySimpleDto>();
        var remoteSalaries = new List<UserSalarySimpleDto>();

        var now = DateTime.UtcNow;
        var weekSplitter = new WeekSplitter(
            now.AddDays(-140),
            now);

        var chart = new SalariesCountWeekByWeekChart(
            localSalaries,
            remoteSalaries,
            weekSplitter,
            true);

        Assert.Equal(20, chart.TotalCountItems.Count);
        foreach (var item in chart.TotalCountItems)
        {
            Assert.Equal(0, item.TotalCount);
            Assert.Equal(0, item.LocalMedian);
            Assert.Equal(0, item.LocalAverage);
            Assert.Equal(0, item.RemoteMedian);
            Assert.Equal(0, item.RemoteAverage);
        }

        Assert.Equal(chart.TotalCountItems.Count * GetSalariesHistoricalChartResponse.GradesToBeUsedInChart.Count, chart.GradeItems.Count);
        foreach (var item in chart.GradeItems)
        {
            Assert.Equal(0, item.TotalCount);
            Assert.Equal(0, item.LocalMedian);
            Assert.Equal(0, item.LocalAverage);
            Assert.Equal(0, item.RemoteMedian);
            Assert.Equal(0, item.RemoteAverage);
        }
    }

    [Fact]
    public void Ctor_HasSalaries_Ok()
    {
        var now = DateTime.UtcNow;
        var weekSplitter = new WeekSplitter(
            now.AddDays(-140),
            now);

        var chart = new SalariesCountWeekByWeekChart(
            _localSalaries,
            _remoteSalaries,
            weekSplitter,
            true);

        Assert.Equal(20, chart.TotalCountItems.Count);

        Assert.Equal(1, chart.TotalCountItems[12].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[12].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[12].LocalAverage);
        Assert.Equal(2_000_000, chart.TotalCountItems[12].RemoteMedian);
        Assert.Equal(2_000_000, chart.TotalCountItems[12].RemoteAverage);

        Assert.Equal(4, chart.TotalCountItems[13].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[13].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[13].LocalAverage);
        Assert.Equal(1_900_000, chart.TotalCountItems[13].RemoteMedian);
        Assert.Equal(1_825_000, chart.TotalCountItems[13].RemoteAverage);

        Assert.Equal(4, chart.TotalCountItems[14].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[14].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[14].LocalAverage);
        Assert.Equal(1_900_000, chart.TotalCountItems[14].RemoteMedian);
        Assert.Equal(1_825_000, chart.TotalCountItems[14].RemoteAverage);

        Assert.Equal(5, chart.TotalCountItems[15].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[15].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[15].LocalAverage);
        Assert.Equal(1_900_000, chart.TotalCountItems[15].RemoteMedian);
        Assert.Equal(1_720_000, chart.TotalCountItems[15].RemoteAverage);

        Assert.Equal(6, chart.TotalCountItems[16].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[16].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[16].LocalAverage);
        Assert.Equal(1_850_000, chart.TotalCountItems[16].RemoteMedian);
        Assert.Equal(1_733_333.3333333333, chart.TotalCountItems[16].RemoteAverage);

        Assert.Equal(8, chart.TotalCountItems[17].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[17].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[17].LocalAverage);
        Assert.Equal(1_850_000, chart.TotalCountItems[17].RemoteMedian);
        Assert.Equal(1_762_500, chart.TotalCountItems[17].RemoteAverage);

        Assert.Equal(8, chart.TotalCountItems[18].TotalCount);
        Assert.Equal(0, chart.TotalCountItems[18].LocalMedian);
        Assert.Equal(0, chart.TotalCountItems[18].LocalAverage);
        Assert.Equal(1_850_000, chart.TotalCountItems[18].RemoteMedian);
        Assert.Equal(1_762_500, chart.TotalCountItems[18].RemoteAverage);

        Assert.Equal(18, chart.TotalCountItems[19].TotalCount);
        Assert.Equal(800_000, chart.TotalCountItems[19].LocalMedian);
        Assert.Equal(755_555.5555555555, chart.TotalCountItems[19].LocalAverage);
        Assert.Equal(1_800_000, chart.TotalCountItems[19].RemoteMedian);
        Assert.Equal(1755555.5555555555, chart.TotalCountItems[19].RemoteAverage);

        Assert.NotEmpty(chart.GradeItems);
        Assert.True(chart.HasGradeItems);
    }

    [Fact]
    public void Ctor_HasSalaries_NoGradeItems_Ok()
    {
        var now = DateTime.UtcNow;
        var weekSplitter = new WeekSplitter(
            now.AddDays(-140),
            now);

        var chart = new SalariesCountWeekByWeekChart(
            _localSalaries,
            _remoteSalaries,
            weekSplitter,
            false);

        Assert.Equal(20, chart.TotalCountItems.Count);

        Assert.Empty(chart.GradeItems);
        Assert.False(chart.HasGradeItems);
    }
}