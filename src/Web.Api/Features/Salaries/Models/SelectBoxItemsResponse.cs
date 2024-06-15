using System.Collections.Generic;
using System.Linq;
using Web.Api.Features.Labels.Models;

namespace Web.Api.Features.Salaries.Models;

public record SelectBoxItemsResponse
{
    public List<LabelEntityDto> Skills { get; init; }

    public List<LabelEntityDto> Industries { get; init; }

    public List<LabelEntityDto> Professions { get; init; }

    public LabelEntityDto GetSkillOrNull(
        long? id)
    {
        if (id == null)
        {
            return null;
        }

        return Skills.FirstOrDefault(x => x.Id == id);
    }

    public LabelEntityDto GetIndustryOrNull(
        long? id)
    {
        if (id == null)
        {
            return null;
        }

        return Industries.FirstOrDefault(x => x.Id == id);
    }

    public LabelEntityDto GetProfessionOrNull(
        long? id)
    {
        if (id == null)
        {
            return null;
        }

        return Professions.FirstOrDefault(x => x.Id == id);
    }
}