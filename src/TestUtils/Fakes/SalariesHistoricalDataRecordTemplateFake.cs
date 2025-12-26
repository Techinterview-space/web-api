using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class SalariesHistoricalDataRecordTemplateFake : SalariesHistoricalDataRecordTemplate
{
    public SalariesHistoricalDataRecordTemplateFake(
        List<long> professionIds = null)
        : base(professionIds ?? new List<long>())
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
