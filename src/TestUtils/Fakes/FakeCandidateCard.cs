using System;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class FakeCandidateCard : CandidateCard, IPlease<CandidateCard>
{
    public FakeCandidateCard(
        Candidate candidate,
        User openBy = null,
        EmploymentStatus employmentStatus = EmploymentStatus.HrInterview)
        : base(candidate, openBy, employmentStatus)
    {
    }

    public CandidateCard Please()
        => this;

    public IPlease<CandidateCard> AsPlease()
    {
        return this;
    }

    public FakeCandidateCard Archived()
    {
        DeletedAt = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1));
        return this;
    }

    public FakeCandidateCard WithFile(
        string fileName)
    {
        AddFile(fileName, Guid.NewGuid() + "/" + fileName);
        return this;
    }

    public async Task<CandidateCard> PleaseAsync(DatabaseContext context)
    {
        var entry = await context.CandidateCards.AddAsync(Please());
        await context.TrySaveChangesAsync();
        return await context.CandidateCards
            .Include(x => x.OpenBy)
            .Include(x => x.Comments)
            .Include(x => x.Organization)
            .Include(x => x.Interviews)
            .ByIdOrFailAsync(entry.Entity.Id);
    }
}