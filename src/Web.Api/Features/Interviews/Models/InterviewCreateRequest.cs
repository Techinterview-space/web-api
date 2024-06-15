using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;
using Domain.Entities.Interviews;
using Web.Api.Features.Labels.Models;

namespace Web.Api.Features.Interviews.Models;

public record InterviewCreateRequest
{
    [Required]
    [StringLength(150)]
    public string CandidateName { get; init; }

    [StringLength(Interview.OverallStringLength)]
    public string OverallOpinion { get; init; }

    public DeveloperGrade? CandidateGrade { get; init; }

    public List<InterviewSubject> Subjects { get; init; } = new ();

    public List<LabelDto> Labels { get; init; } = new ();
}