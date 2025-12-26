namespace Web.Api.Features.SalariesHistoricalDataTemplates.CreateTemplate;

public record CreateSalariesHistoricalDataRecordTemplateCommand
    : CreateSalariesHistoricalDataRecordTemplateBodyRequest
{
    public CreateSalariesHistoricalDataRecordTemplateCommand(
        CreateSalariesHistoricalDataRecordTemplateBodyRequest request)
    {
        ProfessionIds = request.ProfessionIds;
    }
}