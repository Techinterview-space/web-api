using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.SalariesHistoricalDataTemplates.Shared;

public record UpdateSalariesHistoricalDataRecordTemplateBodyRequest
{
    [Required]
    public List<long> ProfessionIds { get; init; } = new List<long>();
}