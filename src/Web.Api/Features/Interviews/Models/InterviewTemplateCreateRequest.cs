using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Interviews;
using TechInterviewer.Features.Labels.Models;

namespace TechInterviewer.Features.Interviews.Models;

public record InterviewTemplateCreateRequest
{
    [Required]
    [StringLength(150)]
    public string Title { get; init; }

    [StringLength(Interview.OverallStringLength)]
    public string OverallOpinion { get; init; }

    public bool IsPublic { get; init; }

    public List<InterviewTemplateSubject> Subjects { get; init; } = new ();

    public List<LabelDto> Labels { get; init; } = new ();
}