using Infrastructure.Salaries;
using Xunit;

namespace InfrastructureTests.Salaries;

public class JobPostingParserTests
{
    [Theory]
    [InlineData("#вакансия Разработчик C# Вилка от 500к до 800к", 500_000d, 800_000d)]
    [InlineData("#вакансия Python разработчик Зарплата 500 000 - 1300000", 500_000d, 1_300_000d)]
    [InlineData("#вакансия Java developer от 200к", 200_000d, null)]
    [InlineData("#вакансия Frontend зп 150 тыс - 250 тысяч", 150_000d, 250_000d)]
    [InlineData("#вакансия Backend developer salary 100000-200000", 100_000d, 200_000d)]
    public void GetResult_HasSalariesInText_Ok(
        string text,
        double? min,
        double? max)
    {
        var result = new JobPostingParser(text).GetResult();

        Assert.True(result.HasHashtag);
        Assert.Equal(min, result.MinSalary);
        Assert.Equal(max, result.MaxSalary);
    }

    [Theory]
    [InlineData("#вакансия без зарплаты")]
    [InlineData("Обычное сообщение без вакансии")]
    public void GetResult_NoSalariesInText_Ok(
        string text)
    {
        var result = new JobPostingParser(text).GetResult();

        Assert.False(result.HasHashtag);
        Assert.Null(result.MinSalary);
        Assert.Null(result.MaxSalary);
    }
}