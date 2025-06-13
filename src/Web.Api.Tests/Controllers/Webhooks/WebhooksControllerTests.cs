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
        var testSignatureKey = "test-signature-key";
        var testBody = "[{\"email\":\"test@example.com\",\"event\":\"delivered\"}]";

        configuration.Setup(x => x["SendGridWebhookSignatureKey"]).Returns(testSignatureKey);

        var controller = new WebhooksController(logger.Object, configuration.Object);

        // Generate valid signature
        var signature = GenerateValidSignature(testSignatureKey, testBody);

        // Mock HttpContext and Request
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["X-Twilio-Email-Event-Webhook-Signature"] = signature
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
        var testSignatureKey = "test-signature-key";
        var testBody = "[{\"email\":\"test@example.com\",\"event\":\"delivered\"}]";
        var invalidSignature = "invalid-signature";

        configuration.Setup(x => x["SendGridWebhookSignatureKey"]).Returns(testSignatureKey);

        var controller = new WebhooksController(logger.Object, configuration.Object);

        // Mock HttpContext and Request
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["X-Twilio-Email-Event-Webhook-Signature"] = invalidSignature
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

        configuration.Setup(x => x["SendGridWebhookSignatureKey"]).Returns((string)null);

        var controller = new WebhooksController(logger.Object, configuration.Object);

        // Mock HttpContext and Request
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            ["X-Twilio-Email-Event-Webhook-Signature"] = "some-signature"
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SendGridWebhookSignatureKey is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static string GenerateValidSignature(string signatureKey, string requestBody)
    {
        var keyBytes = Encoding.UTF8.GetBytes(signatureKey);
        var bodyBytes = Encoding.UTF8.GetBytes(requestBody);

        using var hmac = new HMACSHA256(keyBytes);
        var computedHash = hmac.ComputeHash(bodyBytes);
        return Convert.ToBase64String(computedHash);
    }
}