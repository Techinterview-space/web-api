using Domain.Entities.Salaries;

namespace Infrastructure.Services.Professions;

public interface IProfessionsCacheService
{
    Task<List<Profession>> GetProfessionsAsync(
        CancellationToken cancellationToken = default);
}