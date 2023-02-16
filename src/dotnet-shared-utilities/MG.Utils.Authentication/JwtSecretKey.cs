using System.Text;
using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.IdentityModel.Tokens;

namespace MG.Utils.Authentication
{
    public class JwtSecretKey : SymmetricSecurityKey
    {
        public JwtSecretKey(NonNullableString key)
            : this(key.Value())
        {
        }

        public JwtSecretKey(string key)
            : base(Encoding.UTF8.GetBytes(key))
        {
        }
    }
}