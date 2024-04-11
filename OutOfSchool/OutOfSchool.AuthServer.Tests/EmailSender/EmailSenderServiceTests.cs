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
    private Mock<IScheduler> mockScheduler;
    private EmailSenderService emailSenderService;

    [SetUp]
    public void Setup()
    {
        mockScheduler = new Mock<IScheduler>();
        emailSenderService = new EmailSenderService(mockScheduler.Object);
    }

    [Test]
    public async Task SendAsync_AddsJobToScheduler()
    {
        // Arrange
        string email = "test@example.com";
        string subject = "Test Email";
        var content = ("<html><body><h1>Hello</h1></body></html>", "Hello");

        // Act
        await emailSenderService.SendAsync(email, subject, content);

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
