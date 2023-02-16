using System.ComponentModel.DataAnnotations;

namespace MG.Utils.Attributes
{
    public class OnlyDigitsAttribute : RegularExpressionAttribute
    {
        public OnlyDigitsAttribute(int length)
            : base($@"^([0-9]{{{length}}})$")
        {
        }
    }
}