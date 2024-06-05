using System;

namespace Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GroupAttribute : Attribute
    {
        public string GroupName { get; }

        public GroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
