using System;
using System.Collections.Generic;

namespace Domain.Entities.HistoricalRecords;

public class SalariesHistoricalDataRecordTemplate : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public List<long> ProfessionIds { get; protected set; }

    public virtual List<SalariesHistoricalDataRecord> SalariesHistoricalDataRecords { get; protected set; }

    public SalariesHistoricalDataRecordTemplate(
        List<long> professionIds)
    {
        Id = Guid.NewGuid();
        ProfessionIds = professionIds;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
        SalariesHistoricalDataRecords = new List<SalariesHistoricalDataRecord>();
    }

    public void Update(
        List<long> professionIds)
    {
        ProfessionIds = professionIds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected SalariesHistoricalDataRecordTemplate()
    {
    }
}