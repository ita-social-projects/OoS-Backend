using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender.Quartz;
using OutOfSchool.EmailSender;
using Quartz;
using System.Threading.Tasks;
using System;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Collections.Generic;
using OutOfSchool.EmailSender.Senders;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class EmailSenderJobTests
{
    private const string DateTimeStringFormat = "dd.MM.yyyy HH:mm:ss zzz";
    private Mock<IOptions<EmailOptions>> _mockEmailOptions;
    private Mock<ILogger<EmailSenderJob>> _mockLogger;
    private Mock<IEmailSender> _mockEmailSender;

    private EmailSenderJob _emailSenderJob;

    [SetUp]
    public void Setup()
    {
        _mockEmailOptions = new Mock<IOptions<EmailOptions>>();
        _mockLogger = new Mock<ILogger<EmailSenderJob>>();
        _mockEmailSender = new Mock<IEmailSender>();

        _emailSenderJob = new EmailSenderJob(
            _mockEmailOptions.Object,
            _mockLogger.Object,
            _mockEmailSender.Object
        );
    }

    [Test]
    public async Task Execute_WithEmptyMergedJobData_ShouldNotSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = false });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(x => x.MergedJobDataMap).Returns([]);

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockEmailSender.Verify(
            sender => sender.SendAsync(It.IsAny<SendGridMessage>()),
            Times.Never);
    }

    [Test]
    public async Task Execute_WithDisabledEmailOptions_ShouldNotSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = false });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(x => x.MergedJobDataMap).Returns([new KeyValuePair<string, object>("1", "2")]);

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockEmailSender.Verify(
            sender => sender.SendAsync(It.IsAny<SendGridMessage>()),
            Times.Never);
    }

    [Test]
    public async Task Execute_WithExpiredEmail_ShouldNotSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.MergedJobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddMinutes(-10).ToString(DateTimeStringFormat) }
            });

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockEmailSender.Verify(
            sender => sender.SendAsync(It.IsAny<SendGridMessage>()),
            Times.Never);
    }

    [Test]
    public async Task Execute_WithValidEmail_ShouldSendEmail()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.MergedJobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddDays(1).ToString(DateTimeStringFormat) }
            });
        // _mockEmailSender.Setup(sender => sender.SendAsync(It.IsAny<SendGridMessage>())).ReturnsAsync(new Response(HttpStatusCode.OK, null, null));

        // Act
        await _emailSenderJob.Execute(mockContext.Object);

        // Assert
        _mockEmailSender.Verify(
            sender => sender.SendAsync(It.IsAny<SendGridMessage>()),
            Times.Once);
    }

    [Test]
    public void Execute_WithRateLimitExceeded_ThrowsJobExecutionException()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.MergedJobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("<html><body><h1>Hello</h1></body></html>")) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello")) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddDays(1).ToString(DateTimeStringFormat) }
            });
        _mockEmailSender.Setup(sender => sender.SendAsync(It.IsAny<SendGridMessage>()))
            .ThrowsAsync(new JobExecutionException());

        // Act & Assert
        Assert.ThrowsAsync<JobExecutionException>(() => _emailSenderJob.Execute(mockContext.Object));
    }
    

    [Test]
    public async Task Execute_WithSendGridError_ShoundNotThrowException()
    {
        // Arrange
        _mockEmailOptions.Setup(options => options.Value).Returns(new EmailOptions { Enabled = true });
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(context => context.MergedJobDataMap)
            .Returns(new JobDataMap
            {
                { EmailSenderStringConstants.Email, "test@example.com" },
                { EmailSenderStringConstants.Subject, "Test Email" },
                { EmailSenderStringConstants.HtmlContent, Convert.ToBase64String("<html><body><h1>Hello</h1></body></html>"u8.ToArray()) },
                { EmailSenderStringConstants.PlainContent, Convert.ToBase64String("Hello"u8.ToArray()) },
                { EmailSenderStringConstants.ExpirationTime, DateTimeOffset.Now.AddDays(1).ToString(DateTimeStringFormat) }
            });
        _mockEmailSender.Setup(client => client.SendAsync(It.IsAny<SendGridMessage>()))
            .ThrowsAsync(new Exception());

        // Act
        Assert.ThrowsAsync<JobExecutionException>(() => _emailSenderJob.Execute(mockContext.Object));

        // Assert
        _mockEmailSender.Verify(
            sender => sender.SendAsync(It.IsAny<SendGridMessage>()),
            Times.Once);
    }
}
