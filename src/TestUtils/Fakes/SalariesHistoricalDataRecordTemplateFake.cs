using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class SalariesHistoricalDataRecordTemplateFake : SalariesHistoricalDataRecordTemplate
{
    public SalariesHistoricalDataRecordTemplateFake(
        string name = "Test Template",
        List<long> professionIds = null)
        : base(
            name,
            professionIds ?? new List<long>())
    {
    }

    public async Task<SalariesHistoricalDataRecordTemplate> PleaseAsync(
        DatabaseContext context)
    {
        var entry = await context.SalariesHistoricalDataRecordTemplates.AddAsync(this);
        await context.TrySaveChangesAsync();
        return entry.Entity;
    }
}
