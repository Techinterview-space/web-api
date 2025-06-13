using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Api.Features.Webhooks;
using Xunit;

namespace Web.Api.Tests.Controllers.Webhooks;

public class WebhooksControllerTests
{
    [Fact]
    public async Task SendgridWebhook_ValidSignature_ProcessesWebhook()
    {
        // Arrange
        var logger = new Mock<ILogger<WebhooksController>>();
        var configuration = new Mock<IConfiguration>();
        var testSignature = Guid.NewGuid().ToString();
        var testBody = "[{\"email\":\"test@example.com\",\"event\":\"delivered\"}]";

        configuration.Setup(x => x["SendGridWebhookSignature"]).Returns(testSignature);

        var controller = new WebhooksController(logger.Object, configuration.Object);

        // Mock HttpContext and Request
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["X-Twilio-Email-Event-Webhook-Signature"] = testSignature
        };

        var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(testBody));
        bodyStream.Position = 0;

        request.Setup(x => x.Headers).Returns(headers);
        request.Setup(x => x.Body).Returns(bodyStream);

        httpContext.Setup(x => x.Request).Returns(request.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext.Object };

        // Act
        var result = await controller.SendgridWebhook(CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        // Verify that no warning about signature failure was logged
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("signature verification failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendgridWebhook_InvalidSignature_ReturnsOkWithWarning()
    {
        // Arrange
        var logger = new Mock<ILogger<WebhooksController>>();
        var configuration = new Mock<IConfiguration>();
        var testSignature = Guid.NewGuid().ToString();
        var testBody = "[{\"email\":\"test@example.com\",\"event\":\"delivered\"}]";

        configuration.Setup(x => x["SendGridWebhookSignature"]).Returns(testSignature);

        var controller = new WebhooksController(logger.Object, configuration.Object);

        // Mock HttpContext and Request
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["X-Twilio-Email-Event-Webhook-Signature"] = Guid.NewGuid().ToString()
        };

        var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(testBody));
        bodyStream.Position = 0;

        request.Setup(x => x.Headers).Returns(headers);
        request.Setup(x => x.Body).Returns(bodyStream);

        httpContext.Setup(x => x.Request).Returns(request.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext.Object };

        // Act
        var result = await controller.SendgridWebhook(CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        // Verify that warning about signature failure was logged
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SendGrid webhook signature verification failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendgridWebhook_MissingSignatureKey_ReturnsOkWithWarning()
    {
        // Arrange
        var logger = new Mock<ILogger<WebhooksController>>();
        var configuration = new Mock<IConfiguration>();
        var testBody = "[{\"email\":\"test@example.com\",\"event\":\"delivered\"}]";

        configuration.Setup(x => x["SendGridWebhookSignature"]).Returns((string)null);

        var controller = new WebhooksController(logger.Object, configuration.Object);

        // Mock HttpContext and Request
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["X-Twilio-Email-Event-Webhook-Signature"] = Guid.NewGuid().ToString()
        };

        var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(testBody));
        bodyStream.Position = 0;

        request.Setup(x => x.Headers).Returns(headers);
        request.Setup(x => x.Body).Returns(bodyStream);

        httpContext.Setup(x => x.Request).Returns(request.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext.Object };

        // Act
        var result = await controller.SendgridWebhook(CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        // Verify that warning about missing signature key was logged
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SendGridWebhookSignature is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}