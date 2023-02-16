using MG.Utils.Abstract.NonNullableObjects;
using Xunit;

namespace MG.Utils.Abstract.Test.NonNullableObjects
{
    public class RandomTokenTest
    {
        [Fact]
        public void RandomToken_Ok()
        {
            var token = (string)new RandomToken();
            Assert.NotEmpty(token);
            Assert.Equal(128, token.Length);
        }
    }
}