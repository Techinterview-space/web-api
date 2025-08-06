using Infrastructure.Salaries;
using Xunit;

namespace InfrastructureTests.Salaries;

public class JobPostingParserTests
{
    public const string MultilineText1 = @"#вакансия 1
Вакансия: Middle Frontend Developer

Вилка: 800 000 - 1 300 000 тенге

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.
";

    public const string MultilineText2 = @"#вакансия 2
Вакансия: Middle Frontend Developer

Вилка до 800к

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText3 = @"#вакансия 3
Вакансия: Middle Frontend Developer

Вилка до 800 000 тенге

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText4 = @"#вакансия 4
Вакансия: Middle Frontend Developer

Вилка от 800к

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText5 = @"#вакансия 5
Вакансия: Middle Frontend Developer

Вилка от 800к до 1 300 000 тенге

Проект: цифровая платформа для агросектора: учёт, планирование, аналитика, управление
полями и техникой. Продукт стабильно работает в продакшене и масштабируется.";

    public const string MultilineText6 = @"#вакансия 6
Senior Developer От 1.2млн до 1.9млн
Проект: цифровая платформа для агросектора";

    public const string MultilineText7 = @"#вакансия 7
TeamLead от 2,5млн до 3млн
Проект: цифровая платформа для агросектора";

    public const string MultilineText8 = @"#вакансия 8
Архитектор от 1.8млн
Проект: цифровая платформа для агросектора";

    public const string MultilineText9 = @"#вакансия 9
Архитектор до 1.8млн
Проект: цифровая платформа для агросектора";

    public const string MultilineText10 = @"#вакансия 10
Ищем Middle Frontend-разработчика (Vue 3) в TrustMe - продуктовую IT-команду в Астане!
Формат: офис, 5/2, 8-часовой рабочий день
ЗП на руки от 600.000тг";

    public const string MultilineText11 = @"#вакансия 11
Ищем Middle Frontend-разработчика (Vue 3) в TrustMe - продуктовую IT-команду в Астане!
Формат: офис, 5/2, 8-часовой рабочий день
ЗП на руки от 6.000.000тг";

    public const string MultilineText12 = @"#вакансия 12 #developer #алматы #backend #php

Вакансия: Back-end разработчик
Компания: Adata.kz
Требуемый опыт работы: 1-3 года
Занятость: оффлайн, полный день
Заработная плата: 500 000 - 800 000тг в месяц";

    public const string MultilineText13 = @"#вакансия 13 #developer #алматы #backend #php

Вакансия: Back-end разработчик
Компания: Adata.kz
Требуемый опыт работы: 1-3 года
Занятость: оффлайн, полный день
Заработная плата: 500 000тг в месяц";

    public const string MultilineText14 = @"#вакансия 14 #developer #алматы #backend #php

Вакансия: Back-end разработчик
Компания: Adata.kz
Требуемый опыт работы: 1-3 года
Занятость: оффлайн, полный день
Заработная плата: по договоренности";

    public const string MultilineText15 = @"#вакансия 15 #nextjs #frontend #удаленно
Next.js Developer на проект
Оплата: 100 000 KZT
Сроки: 1-2 недели
Формат: Удалённо, проектная работа
Контакты для отклика: @zhumabekov_04
О проекте:
Ищем Frontend-разработчика для связки веб-платформы на Next.js.
Вся верстка уже готова — нужно подключить backend API и
реализовать логику отображения.
Требования:
- Уверенное знание Next.js и React
- Опыт работы с REST API
- Ответственность и соблюдение сроков
Обязанности:
- Интеграция АРІ с готовыми страницами
- Настройка роутинга и состояния
- Отладка и тестирование";

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
    [InlineData(MultilineText6, 1_200_000d, 1_900_000d)]
    [InlineData(MultilineText7, 2_500_000d, 3_000_000d)]
    [InlineData(MultilineText8, 1_800_000d, null)]
    [InlineData(MultilineText9, null, 1_800_000d)]
    [InlineData(MultilineText10, 600_000d, null)]
    [InlineData(MultilineText11, 6_000_000d, null)]
    [InlineData(MultilineText12, 500_000d, 800_000d)]
    [InlineData(MultilineText13, 500_000d, null)]
    [InlineData(MultilineText14, null, null)]
    [InlineData(MultilineText15, 100_000d, null)]
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