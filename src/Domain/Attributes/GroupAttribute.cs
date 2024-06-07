using System;
using Domain.Entities.Enums;
using Domain.Enums;

namespace Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GroupAttribute : Attribute
    {
        public GradeGroup GroupName { get; }

        public GroupAttribute(GradeGroup groupName)
        {
            GroupName = groupName;
        }
    }
}
