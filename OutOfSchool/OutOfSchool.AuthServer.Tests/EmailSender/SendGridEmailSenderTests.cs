using Moq;
using NUnit.Framework;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OutOfSchool.EmailSender.Senders;
using Quartz;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class SendGridEmailSenderTests
{
    private Mock<ISendGridClient> _sendGridClientMock;
    private SendGridEmailSender _emailSender;

    [SetUp]
    public void Setup()
    {
        _sendGridClientMock = new Mock<ISendGridClient>();
        _emailSender = new SendGridEmailSender(_sendGridClientMock.Object);
    }

    [Test]
    public async Task SendAsync_SendsEmailSuccessfully_WhenResponseIsSuccessful()
    {
        // Arrange
        var sendGridMessage = new SendGridMessage();
        var response = new Response(HttpStatusCode.OK, new StringContent(string.Empty), null);
        _sendGridClientMock
            .Setup(client => client.SendEmailAsync(sendGridMessage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _emailSender.SendAsync(sendGridMessage));
    }

    [Test]
    public void SendAsync_ThrowsJobExecutionException_WhenRateLimitExceeded()
    {
        // Arrange
        var sendGridMessage = new SendGridMessage();
        var response = new Response(HttpStatusCode.TooManyRequests, new StringContent(string.Empty), null);
        _sendGridClientMock
            .Setup(client => client.SendEmailAsync(sendGridMessage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act & Assert
        var ex = Assert.ThrowsAsync<JobExecutionException>(() => _emailSender.SendAsync(sendGridMessage));
        Assert.AreEqual("Email sending rate limit exceeded.", ex.Message);
    }

    [Test]
    public void SendAsync_ThrowsException_WhenResponseIndicatesFailure()
    {
        // Arrange
        var sendGridMessage = new SendGridMessage();
        var errorMessage = "Invalid email address";
        var response = new Response(HttpStatusCode.BadRequest, new StringContent(errorMessage), null);
        
        _sendGridClientMock
            .Setup(client => client.SendEmailAsync(sendGridMessage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _emailSender.SendAsync(sendGridMessage));
        Assert.AreEqual($"Email was not sent with the following error: {errorMessage}", ex.Message);
    }
}
