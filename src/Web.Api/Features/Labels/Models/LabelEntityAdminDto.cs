namespace Web.Api.Features.Labels.Models;

public record LabelEntityAdminDto : LabelEntityDto
{
    public long? CreatedById { get; set; }

    public string CreatedBy { get; set; }
}