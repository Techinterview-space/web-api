using System;

namespace Domain.Entities.Vacancies;

public class VacancyHistory
{
    public VacancyHistory(
        Vacancy vacancy,
        string message,
        string createdBy)
    {
        Vacancy = vacancy;
        VacancyId = vacancy.Id;
        Message = message;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected VacancyHistory()
    {
    }

    public Guid Id { get; protected set; }

    public Guid VacancyId { get; protected set; }

    public virtual Vacancy Vacancy { get; protected set; }

    public string Message { get; protected set; }

    public string CreatedBy { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }
}
