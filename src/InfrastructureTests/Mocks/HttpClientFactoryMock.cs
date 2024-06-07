using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace InfrastructureTests.Mocks
{
    public class HttpClientFactoryMock : Mock<IHttpClientFactory>
    {
        public HttpClientFactoryMock(
            string respData)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(respData)
                });

            var httpClient = new HttpClient(mockHandler.Object);

            Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
        }
    }
}
