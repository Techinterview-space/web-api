using System.ComponentModel.DataAnnotations;

namespace MG.Utils.Attributes
{
    public class StandardStringLengthAttribute : StringLengthAttribute
    {
        public StandardStringLengthAttribute(int length = 255)
            : base(length)
        {
        }
    }
}