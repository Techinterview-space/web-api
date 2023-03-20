using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class FakeCandidate : Candidate, IPlease<Candidate>
{
    public FakeCandidate(
        Organization organization,
        User createdByOrNull = null)
        : base(
            Faker.Name.First(),
            Faker.Name.Last(),
            "Contacts",
            organization.Id,
            createdByOrNull)
    {
    }

    public Candidate Please()
    {
        return this;
    }

    public IPlease<Candidate> AsPlease()
    {
        return this;
    }

    public async Task<Candidate> PleaseAsync(DatabaseContext context)
    {
        var entry = await context.Candidates.AddAsync(Please());
        await context.TrySaveChangesAsync();
        return await context.Candidates
            .Include(x => x.CreatedBy)
            .Include(x => x.Organization)
            .ByIdOrFailAsync(entry.Entity.Id);
    }
}