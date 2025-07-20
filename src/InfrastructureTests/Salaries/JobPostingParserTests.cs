using Infrastructure.Salaries;
using Xunit;

namespace InfrastructureTests.Salaries;

public class JobPostingParserTests
{
    public const string MultilineText1 = @"#вакансия 1
📣Вакансия: Middle Frontend Developer

Вилка: 800 000 - 1 300 000 тенге

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.
";

    public const string MultilineText2 = @"#вакансия 2
📣Вакансия: Middle Frontend Developer

Вилка до 800к

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText3 = @"#вакансия 3
📣Вакансия: Middle Frontend Developer

Вилка до 800 000 тенге

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText4 = @"#вакансия 4
📣Вакансия: Middle Frontend Developer

Вилка от 800к

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText5 = @"#вакансия 5
📣Вакансия: Middle Frontend Developer

Вилка от 800к до 1 300 000 тенге

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

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
    [InlineData(MultilineText1, 800_000d, 1_300_000d)]
    [InlineData(MultilineText2, null, 800_000d)]
    [InlineData(MultilineText3, null, 800_000d)]
    [InlineData(MultilineText4, 800_000d, null)]
    [InlineData(MultilineText5, 800_000d, 1_300_000d)]
    public void MultilineText_Cases_Match(
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
    [InlineData("вакансия без зарплаты")]
    [InlineData("Обычное сообщение без вакансии")]
    public void GetResult_NoSalariesInText_Ok(
        string text)
    {
        var result = new JobPostingParser(text).GetResult();

        Assert.False(result.HasHashtag);
        Assert.Null(result.MinSalary);
        Assert.Null(result.MaxSalary);
    }

    [Fact]
    public void GetResult_HasHashatButNoSalaries_Ok()
    {
        var result = new JobPostingParser("Обычная #вакансия без зарплаты").GetResult();

        Assert.True(result.HasHashtag);
        Assert.False(result.HasAnySalary());
        Assert.Null(result.MinSalary);
        Assert.Null(result.MaxSalary);
    }
}