using System.ComponentModel;
using Domain.Attributes;
using Domain.Enums;

namespace Domain.Entities.Enums;

public enum DeveloperGrade : long
{
    [Description("Unknown")]
    Unknown = 0,

    [Group(GradeGroup.Trainee)]
    [Description("Trainee")]
    Trainee = 1,

    [Group(GradeGroup.Junior)]
    [Description("Junior")]
    Junior = 2,

    [Group(GradeGroup.Middle)]
    [Description("Middle")]
    Middle = 5,

    [Group(GradeGroup.Senior)]
    [Description("Senior")]
    Senior = 8,

    [Group(GradeGroup.Lead)]
    [Description("Lead")]
    Lead = 11,
}