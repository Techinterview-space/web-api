using System;
using Domain.Entities.StatData.Salary;

namespace Domain.Entities.HistoricalRecords;

public class SalariesHistoricalDataRecord : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public DateTimeOffset Date { get; protected set; }

    public Guid TemplateId { get; protected set; }

    public virtual SalariesHistoricalDataRecordTemplate SalariesHistoricalDataRecordTemplate { get; protected set; }

    public SalariesStatDataCacheItemSalaryData Data { get; protected set; }

    public SalariesHistoricalDataRecord(
        DateTimeOffset date,
        Guid templateId,
        SalariesStatDataCacheItemSalaryData data)
    {
        Id = Guid.NewGuid();
        Date = date;
        TemplateId = templateId;
        Data = data;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected SalariesHistoricalDataRecord()
    {
    }
}