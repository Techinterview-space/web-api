using System.ComponentModel;

namespace Domain.Entities.Enums;

public enum DeveloperGrade : long
{
    [Description("Unknown")]
    Unknown = 0,

    [Description("Trainee")]
    Trainee = 1,

    [Description("Junior")]
    Junior = 2,

    [Description("Junior strong")]
    JuniorStrong = 3,

    [Description("Middle minus")]
    MiddleMinus = 4,

    [Description("Middle")]
    Middle = 5,

    [Description("Middle strong")]
    MiddleStrong = 6,

    [Description("Senior minus")]
    SeniorMinus = 7,

    [Description("Senior")]
    Senior = 8,

    [Description("Senior strong")]
    SeniorStrong = 9,

    [Description("Lead minus")]
    LeadMinus = 10,

    [Description("Lead")]
    Lead = 11,

    [Description("Lead strong")]
    LeadStrong = 12,
}