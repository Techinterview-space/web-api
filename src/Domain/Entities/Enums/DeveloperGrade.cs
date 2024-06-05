using System.ComponentModel;
using Domain.Attributes;

namespace Domain.Entities.Enums;

public enum DeveloperGrade : long
{
    [Description("Unknown")]
    Unknown = 0,

    [Description("Trainee")]
    Trainee = 1,

    [Group("Junior")]
    [Description("Junior")]
    Junior = 2,

    [Group("Junior")]
    [Description("Junior strong")]
    JuniorStrong = 3,

    [Group("Middle")]
    [Description("Middle minus")]
    MiddleMinus = 4,

    [Group("Middle")]
    [Description("Middle")]
    Middle = 5,

    [Group("Middle")]
    [Description("Middle strong")]
    MiddleStrong = 6,

    [Group("Senior")]
    [Description("Senior minus")]
    SeniorMinus = 7,

    [Group("Senior")]
    [Description("Senior")]
    Senior = 8,

    [Group("Senior")]
    [Description("Senior strong")]
    SeniorStrong = 9,

    [Group("Lead")]
    [Description("Lead minus")]
    LeadMinus = 10,

    [Group("Lead")]
    [Description("Lead")]
    Lead = 11,

    [Group("Lead")]
    [Description("Lead strong")]
    LeadStrong = 12,
}