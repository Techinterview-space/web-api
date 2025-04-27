using System.Collections.Generic;
using Domain.Entities.Companies;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class CompanyFake : Company
{
    public CompanyFake()
        : base(
            "Company name",
            "Company description",
            new List<string>()
            {
                "https://example.com",
            },
            "https://example.com/logo.png")
    {
    }

    public Company Please(
        InMemoryDatabaseContext context)
    {
        var entry = context.Companies.Add((Company)this);
        context.SaveChanges();

        return entry.Entity;
    }
}