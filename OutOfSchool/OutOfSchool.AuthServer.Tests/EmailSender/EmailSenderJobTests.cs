using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender.Quartz;
using OutOfSchool.EmailSender;
using Quartz;
using SendGrid;
using System.Threading.Tasks;
using System;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Text;
using System.Threading;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class EmailSenderJobTests
{
    private Mock<IOptions<EmailOptions>> _mockEmailOptions;
    private Mock<ILogger<EmailSenderJob>> _mockLogger;
    private Mock<ISendGridClient> _mockSendGridClient;

    private EmailSenderJob _emailSenderJob;

    [SetUp]
    public void Setup()
    {
        _mockEmailOptions = new Mock<IOptions<EmailOptions>>();
        _mockLogger = new Mock<ILogger<EmailSenderJob>>();
        _mockSendGridClient = new Mock<ISendGridClient>();

        _emailSenderJob = new EmailSenderJob(
            _mockEmailOptions.Object,
            _mockLogger.Object,
            _mockSendGridClient.Object
        );
    }

    [Test]
    public async Task Execute_WithDisabledEmailOptions_ShouldNotSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = false });
        var mockContext = new Mock<IJobExecutionContext>();

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockSendGridClient.Verify(
            client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Execute_WithExpiredEmail_ShouldNotSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.JobDetail.JobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddMinutes(-10) }
            });

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockSendGridClient.Verify(
            client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Execute_WithValidEmail_ShouldSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.JobDetail.JobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddDays(1) }
            });
        _mockSendGridClient.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, null, null));

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockSendGridClient.Verify(
            client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Execute_WithRateLimitExceeded_ThrowsJobExecutionException()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.JobDetail.JobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddDays(1) }
            });
        _mockSendGridClient.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.TooManyRequests, null, null));

        // Act & Assert
        Assert.ThrowsAsync<JobExecutionException>(() => _emailSenderJob.Execute(mockContext.Object));
    }

    [Test]
    public void Execute_WithSendGridError_ShouldThrowException()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.JobDetail.JobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddDays(1) }
            });
        _mockSendGridClient.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.BadRequest, null, null));

        // Assert
        _mockSendGridClient.Verify(
            client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
