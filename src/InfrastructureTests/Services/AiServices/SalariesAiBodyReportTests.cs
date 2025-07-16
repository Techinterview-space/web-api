using System.Text.Json;
using Infrastructure.Services.AiServices.Salaries;
using Xunit;

namespace InfrastructureTests.Services.AiServices;

public class SalariesAiBodyReportTests
{
    private const string Json = @"
{
  ""reportMetadata"": {
    ""reportDate"": ""2025-07-16"",
    ""currency"": ""KZT"",
    ""periodType"": ""weekly""
  },
  ""roles"": [
    {
      ""roleName"": ""Pentester"",
      ""currentSalary"": {
        ""average"": 514255.5,
        ""median"": 452500,
        ""min"": 75000,
        ""max"": 1500000,
        ""count"": 10
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 514255.5,
          ""median"": 452500,
          ""count"": 10,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 514255.5,
          ""median"": 452500,
          ""count"": 10,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 514255.5,
          ""median"": 452500,
          ""count"": 10,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Backend Developer"",
      ""currentSalary"": {
        ""average"": 815674.4941176471,
        ""median"": 700000,
        ""min"": 75000,
        ""max"": 4800000,
        ""count"": 510
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 816687.6070726915,
          ""median"": 700000,
          ""count"": 509,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 815662.6825396825,
          ""median"": 700000,
          ""count"": 504,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 811711.4068136272,
          ""median"": 700000,
          ""count"": 499,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Frontend Developer"",
      ""currentSalary"": {
        ""average"": 772498.4954954955,
        ""median"": 680000,
        ""min"": 75000,
        ""max"": 2800000,
        ""count"": 333
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 772498.4954954955,
          ""median"": 680000,
          ""count"": 333,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 768115.8506097561,
          ""median"": 650000,
          ""count"": 328,
          ""percentChange"": -0.01
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 766489.2935779816,
          ""median"": 650000,
          ""count"": 327,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Тестировщик (Tester)"",
      ""currentSalary"": {
        ""average"": 423412.4166666667,
        ""median"": 375000,
        ""min"": 75000,
        ""max"": 1000000,
        ""count"": 36
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 423412.4166666667,
          ""median"": 375000,
          ""count"": 36,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 412652.77142857143,
          ""median"": 370000,
          ""count"": 35,
          ""percentChange"": -0.03
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 412652.77142857143,
          ""median"": 370000,
          ""count"": 35,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Архитектор (Architect)"",
      ""currentSalary"": {
        ""average"": 1503294.1176470588,
        ""median"": 1386000,
        ""min"": 500000,
        ""max"": 3500000,
        ""count"": 17
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 1503294.1176470588,
          ""median"": 1386000,
          ""count"": 17,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 1503294.1176470588,
          ""median"": 1386000,
          ""count"": 17,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 1503294.1176470588,
          ""median"": 1386000,
          ""count"": 17,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Quality Assurance (QA)"",
      ""currentSalary"": {
        ""average"": 774418.0327868853,
        ""median"": 731000,
        ""min"": 150000,
        ""max"": 2100000,
        ""count"": 122
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 774418.0327868853,
          ""median"": 731000,
          ""count"": 122,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 774418.0327868853,
          ""median"": 731000,
          ""count"": 122,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 773158.3333333334,
          ""median"": 731000,
          ""count"": 120,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Техлид (Techleader)"",
      ""currentSalary"": {
        ""average"": 2093000,
        ""median"": 2000000,
        ""min"": 1000000,
        ""max"": 2900000,
        ""count"": 13
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 2093000,
          ""median"": 2000000,
          ""count"": 13,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 2093000,
          ""median"": 2000000,
          ""count"": 13,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 2093000,
          ""median"": 2000000,
          ""count"": 13,
          ""percentChange"": 0
        }
      ]
    },
    {
      ""roleName"": ""Тимлид | Teamleader"",
      ""currentSalary"": {
        ""average"": 1366613.6363636365,
        ""median"": 1300000,
        ""min"": 100000,
        ""max"": 3200000,
        ""count"": 88
      },
      ""historicalData"": [
        {
          ""date"": ""2025-07-09"",
          ""average"": 1361632.183908046,
          ""median"": 1300000,
          ""count"": 87,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-07-02"",
          ""average"": 1361632.183908046,
          ""median"": 1300000,
          ""count"": 87,
          ""percentChange"": 0
        },
        {
          ""date"": ""2025-06-25"",
          ""average"": 1356534.8837209302,
          ""median"": 1300000,
          ""count"": 86,
          ""percentChange"": 0
        }
      ]
    }
  ]
}";

    [Fact]
    public void ParseTest()
    {
        var target = JsonSerializer.Deserialize<SalariesAiBodyReport>(Json);

        Assert.NotNull(target);
        Assert.Equal(8, target.Roles.Count);
    }
}