namespace Web.Api.Features.SalariesHistoricalDataTemplates.CreateTemplate;

public record CreateSalariesHistoricalDataRecordTemplateCommand
    : CreateSalariesHistoricalDataRecordTemplateBodyRequest
{
    public CreateSalariesHistoricalDataRecordTemplateCommand(
        CreateSalariesHistoricalDataRecordTemplateBodyRequest request)
    {
        Name = request.Name;
        ProfessionIds = request.ProfessionIds;
    }
}