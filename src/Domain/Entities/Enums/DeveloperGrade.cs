using System.ComponentModel;

namespace Domain.Entities.Enums;

public enum DeveloperGrade : long
{
    [Description("Unknown")]
    Unknown = 0,

    [Description("Junior-")]
    JuniorMinus = 1,

    [Description("Junior")]
    Junior = 2,

    [Description("Junior+")]
    JuniorStrong = 3,

    [Description("Middle-")]
    MiddleMinus = 4,

    [Description("Middle")]
    Middle = 5,

    [Description("Middle+")]
    MiddleStrong = 6,

    [Description("Senior-")]
    SeniorMinus = 7,

    [Description("Senior")]
    Senior = 8,

    [Description("Senior+")]
    SeniorStrong = 9,

    [Description("Lead-")]
    LeadMinus = 10,

    [Description("Lead")]
    Lead = 11,

    [Description("Lead+")]
    LeadStrong = 12,
}