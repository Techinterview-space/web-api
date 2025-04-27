using System;

namespace Domain.Entities.Companies;

public record CompanyRatingHistory
{
    public CompanyRatingHistory(
        double rating,
        Company company)
    {
        Rating = rating;
        CompanyId = company.Id;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public double Rating { get; protected set; }

    public Guid CompanyId { get; protected set; }

    public virtual Company Company { get; protected set; }

    protected CompanyRatingHistory()
    {
    }
}