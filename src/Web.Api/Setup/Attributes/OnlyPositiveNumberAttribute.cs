using System.ComponentModel.DataAnnotations;

namespace Web.Api.Setup.Attributes;

public class OnlyPositiveNumberAttribute : RangeAttribute
{
    public OnlyPositiveNumberAttribute()
        : base(0, double.MaxValue)
    {
    }
}