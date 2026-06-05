using System;
using Domain.Entities.Companies;
using Domain.Entities.Users;
using Domain.Entities.Vacancies;
using Domain.Enums;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class VacancyFake : Vacancy
{
    public VacancyFake(
        Company company,
        User author,
        VacancyStatus status = VacancyStatus.Public,
        string title = null,
        string description = null,
        string hrContact = null,
        string companyName = null,
        bool hideAttachedCompany = false)
        : base(
            title ?? "Vacancy title",
            company,
            author,
            hrContact,
            description ?? "Vacancy description",
            status,
            companyName,
            hideAttachedCompany)
    {
    }

    public VacancyFake SetCreatedAt(
        DateTimeOffset date)
    {
        CreatedAt = date;
        return this;
    }

    public Vacancy Please(
        InMemoryDatabaseContext context)
    {
        var entry = context.Vacancies.Add((Vacancy)this);
        context.SaveChanges();

        return entry.Entity;
    }
}
