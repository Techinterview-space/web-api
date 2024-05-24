using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Domain.Entities.Interviews;

public record InterviewSubject
{
    public InterviewSubject()
    {
    }

    public InterviewSubject(
        string title,
        DeveloperGrade? grade,
        string comments)
    {
        Title = title;
        Grade = grade;
        Comments = comments;
    }

    [Required]
    [StringLength(300)]
    public string Title { get; init; }

    public DeveloperGrade? Grade { get; init; }

    [StringLength(10_000)]
    public string Comments { get; init; }
}