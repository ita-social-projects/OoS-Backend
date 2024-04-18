using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender.Services;
using System.Text;
using System.Threading.Tasks;
using System;
using Quartz;
using System.Threading;
using Quartz.Impl;
using OutOfSchool.EmailSender;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class EmailSenderServiceTests
{
    private Mock<ISchedulerFactory> mockSchedulerFactory;
    private Mock<IScheduler> mockScheduler;
    private EmailSenderService emailSenderService;

    [SetUp]
    public void Setup()
    {
        mockSchedulerFactory = new Mock<ISchedulerFactory>();
        mockScheduler = new Mock<IScheduler>();
        emailSenderService = new EmailSenderService(mockSchedulerFactory.Object);
    }

    [Test]
    public async Task SendAsync_AddsJobToScheduler()
    {
        // Arrange
        string email = "test@example.com";
        string subject = "Test Email";
        var content = ("<html><body><h1>Hello</h1></body></html>", "Hello");
        var expirationTime = DateTimeOffset.Now.AddHours(1);
        mockSchedulerFactory.Setup(f => f.GetScheduler(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockScheduler.Object);

        // Act
        await emailSenderService.SendAsync(email, subject, content, expirationTime);

        // Assert
        mockScheduler.Verify(
            scheduler => scheduler.AddJob(It.IsAny<IJobDetail>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SendAsync_EncodesContentBeforeAddingJob()
    {
        // Arrange
        string email = "test@example.com";
        string subject = "Test Email";
        var content = ("<html><body><h1>Hello</h1></body></html>", "Hello");
        string encodedHtml = Convert.ToBase64String(Encoding.ASCII.GetBytes(content.Item1));
        string encodedPlain = Convert.ToBase64String(Encoding.ASCII.GetBytes(content.Item2));
        mockSchedulerFactory.Setup(f => f.GetScheduler(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockScheduler.Object);

        // Act
        await emailSenderService.SendAsync(email, subject, content);

        // Assert
        mockScheduler.Verify(
            scheduler => scheduler.AddJob(It.Is<JobDetailImpl>(job =>
                    job.JobDataMap[EmailSenderStringConstants.Email].Equals(email) &&
                    job.JobDataMap[EmailSenderStringConstants.Subject].Equals(subject) &&
                    job.JobDataMap[EmailSenderStringConstants.HtmlContent].Equals(encodedHtml) &&
                    job.JobDataMap[EmailSenderStringConstants.PlainContent].Equals(encodedPlain)),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}