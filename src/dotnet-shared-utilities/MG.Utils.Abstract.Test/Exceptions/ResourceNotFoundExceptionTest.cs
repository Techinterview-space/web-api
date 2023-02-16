using MG.Utils.Abstract.Exceptions;
using Xunit;

namespace MG.Utils.Abstract.Test.Exceptions
{
    public class ResourceNotFoundExceptionTest
    {
        [Fact]
        public void CreateFromEntity_ExpectedMessage_Ok()
        {
            const long entityId = 1;
            ResourceNotFoundException exception = ResourceNotFoundException.CreateFromEntity<AwesomeEntity>(entityId);

            Assert.Equal(
                $"Did not find any {typeof(AwesomeEntity).Name} by id={entityId}",
                exception.Message);
        }

        public class AwesomeEntity
        {
        }
    }
}