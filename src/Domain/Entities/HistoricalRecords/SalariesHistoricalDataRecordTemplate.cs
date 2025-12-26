using System;
using System.Collections.Generic;
using Domain.Validation.Exceptions;

namespace Domain.Entities.HistoricalRecords;

public class SalariesHistoricalDataRecordTemplate : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public List<long> ProfessionIds { get; protected set; }

    public virtual List<SalariesHistoricalDataRecord> SalariesHistoricalDataRecords { get; protected set; }

    public SalariesHistoricalDataRecordTemplate(
        string name,
        List<long> professionIds)
    {
        name = name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new BadRequestException("Name cannot be null or empty.");
        }

        Id = Guid.NewGuid();
        Name = name;
        ProfessionIds = professionIds;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
        SalariesHistoricalDataRecords = new List<SalariesHistoricalDataRecord>();
    }

    public void Update(
        string name,
        List<long> professionIds)
    {
        Name = name;
        ProfessionIds = professionIds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected SalariesHistoricalDataRecordTemplate()
    {
    }
}