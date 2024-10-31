using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.EmailSender.Senders;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class DevelopmentEmailSenderTests
{
    private Mock<ILogger<DevelopmentEmailSender>> _loggerMock;
    private DevelopmentEmailSender _emailSender;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<DevelopmentEmailSender>>();
        _emailSender = new DevelopmentEmailSender(_loggerMock.Object);
    }

    [Test]
    public async Task SendAsync_LogsEmailDetailsWithExpirationTime_WhenExpirationTimeIsProvided()
    {
        // Arrange
        var sendGridMessage = new SendGridMessage
        {
            ReplyTo = new EmailAddress("test@example.com"),
            Subject = "Test Subject",
            HtmlContent = "Test Content",
            CustomArgs = new Dictionary<string, string> { { "expirationTime", "2024-12-31T23:59:59Z" } }
        };

        // Act
        await _emailSender.SendAsync(sendGridMessage);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains("Sending mail to test@example.com with subject 'Test Subject'") &&
                    v.ToString().Contains("content: Test Content") &&
                    v.ToString().Contains("expirationTime: 2024-12-31T23:59:59Z")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task SendAsync_LogsEmailDetailsWithCurrentTime_WhenExpirationTimeIsNotProvided()
    {
        // Arrange
        var sendGridMessage = new SendGridMessage
        {
            ReplyTo = new EmailAddress("test@example.com"),
            Subject = "Test Subject",
            HtmlContent = "Test Content",
            CustomArgs = new Dictionary<string, string>
            {
                {"expirationTime", ""}
            }
        };

        // Act
        await _emailSender.SendAsync(sendGridMessage);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains("Sending mail to test@example.com with subject 'Test Subject'") &&
                    v.ToString().Contains("content: Test Content") &&
                    v.ToString().Contains("expirationTime:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}