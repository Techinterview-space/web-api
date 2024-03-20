using Domain.Exceptions;
using Xunit;

namespace InfrastructureTests.Exceptions;

public class NotFoundExceptionTest
{
    [Fact]
    public void CreateFromEntity_ExpectedMessage_Ok()
    {
        const long entityId = 1;
        NotFoundException exception = NotFoundException.CreateFromEntity<AwesomeEntity>(entityId);

        Assert.Equal(
            $"Did not find any {typeof(AwesomeEntity).Name} by id={entityId}",
            exception.Message);
    }

    public class AwesomeEntity
    {
    }
}