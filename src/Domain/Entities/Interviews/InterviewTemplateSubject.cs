using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Interviews;

public record InterviewTemplateSubject
{
    public InterviewTemplateSubject()
    {
    }

    public InterviewTemplateSubject(
        string title,
        string description = null)
    {
        Title = title;
        Description = description;
    }

    [Required]
    [StringLength(150)]
    public string Title { get; init; }

    [StringLength(2000)]
    public string Description { get; init; }
}