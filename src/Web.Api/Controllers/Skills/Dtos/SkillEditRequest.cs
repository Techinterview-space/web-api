using System.ComponentModel.DataAnnotations;

namespace TechInterviewer.Controllers.Skills.Dtos;

public record SkillEditRequest
{
    public long? Id { get; init; }

    [Required]
    [StringLength(50)]
    public string Title { get; init; }

    [Required]
    [StringLength(7)]
    public string HexColor { get; init; }
}