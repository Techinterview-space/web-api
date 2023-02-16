using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Employments;
using Domain.Services.Labels;
using MG.Utils.Attributes;

namespace Domain.Services.Organizations.Requests;

public record EditCandidateCardRequest : EditCandidateCardEmploymentStatusRequest
{
    public Guid? CandidateId { get; init; }

    [Required]
    [StringLength(300)]
    public string CandidateFirstName { get; init; }

    [Required]
    [StringLength(300)]
    public string CandidateLastName { get; init; }

    [StringLength(1000)]
    public string CandidateContacts { get; init; }

    public List<LabelDto> Labels { get; init; } = new ();
}

public record EditCandidateCardEmploymentStatusRequest
{
    [NotDefaultValue]
    public EmploymentStatus EmploymentStatus { get; init; }

    [NotDefaultValue]
    public Guid OrganizationId { get; init; }
}