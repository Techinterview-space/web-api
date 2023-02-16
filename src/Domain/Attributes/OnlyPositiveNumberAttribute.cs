using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Attributes;

public class OnlyPositiveNumberAttribute : RangeAttribute
{
    public OnlyPositiveNumberAttribute()
        : base(0, double.MaxValue)
    {
    }
}