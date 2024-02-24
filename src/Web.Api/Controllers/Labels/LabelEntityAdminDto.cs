namespace TechInterviewer.Controllers.Labels;

public record LabelEntityAdminDto : LabelEntityDto
{
    public long? CreatedById { get; set; }

    public string CreatedBy { get; set; }
}