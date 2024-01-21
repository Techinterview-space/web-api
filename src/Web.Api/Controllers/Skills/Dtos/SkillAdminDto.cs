namespace TechInterviewer.Controllers.Skills.Dtos;

public record SkillAdminDto : SkillDto
{
    public long? CreatedById { get; set; }

    public string CreatedBy { get; set; }
}