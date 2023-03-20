using System.ComponentModel.DataAnnotations;

namespace TechInterviewer.Setup.Attributes;

public class OnlyPositiveNumberAttribute : RangeAttribute
{
    public OnlyPositiveNumberAttribute()
        : base(0, double.MaxValue)
    {
    }
}