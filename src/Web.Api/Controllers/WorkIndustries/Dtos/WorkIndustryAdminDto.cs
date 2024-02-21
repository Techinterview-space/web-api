namespace TechInterviewer.Controllers.WorkIndustries.Dtos;

public record WorkIndustryAdminDto : WorkIndustryDto
{
    public long? CreatedById { get; set; }

    public string CreatedBy { get; set; }
}