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

    [Group(GradeGroup.Junior)]
    [Description("Junior strong")]
    JuniorStrong = 3,

    [Group(GradeGroup.Middle)]
    [Description("Middle minus")]
    MiddleMinus = 4,

    [Group(GradeGroup.Middle)]
    [Description("Middle")]
    Middle = 5,

    [Group(GradeGroup.Middle)]
    [Description("Middle strong")]
    MiddleStrong = 6,

    [Group(GradeGroup.Senior)]
    [Description("Senior minus")]
    SeniorMinus = 7,

    [Group(GradeGroup.Senior)]
    [Description("Senior")]
    Senior = 8,

    [Group(GradeGroup.Senior)]
    [Description("Senior strong")]
    SeniorStrong = 9,

    [Group(GradeGroup.Lead)]
    [Description("Lead minus")]
    LeadMinus = 10,

    [Group(GradeGroup.Lead)]
    [Description("Lead")]
    Lead = 11,

    [Group(GradeGroup.Lead)]
    [Description("Lead strong")]
    LeadStrong = 12,
}