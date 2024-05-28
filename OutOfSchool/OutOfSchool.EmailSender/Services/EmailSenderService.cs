using System;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.EmailSender.Quartz;
using Quartz;

namespace OutOfSchool.EmailSender.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly ISchedulerFactory schedulerFactory;
    private readonly ISendGridAccessibilityService sendGridAccessibilityService;

    public EmailSenderService(
        ISchedulerFactory schedulerFactory,
        ISendGridAccessibilityService sendGridAccessibilityService)
    {
        this.schedulerFactory = schedulerFactory;
        this.sendGridAccessibilityService = sendGridAccessibilityService;
    }

    public async Task SendAsync(string email, string subject, (string html, string plain) content, DateTimeOffset? expirationTime = null)
    {
        expirationTime ??= DateTimeOffset.MaxValue;

        var jobData = new JobDataMap
        {
            { EmailSenderStringConstants.Email, email },
            { EmailSenderStringConstants.Subject, subject },
            { EmailSenderStringConstants.HtmlContent, EncodeToBase64(content.html) },
            { EmailSenderStringConstants.PlainContent, EncodeToBase64(content.plain) },
            { EmailSenderStringConstants.ExpirationTime, ((DateTimeOffset)expirationTime).ToString("dd.MM.yyyy HH:mm:ss zzz") },
        };

        var scheduler = await schedulerFactory.GetScheduler();

        var job = JobBuilder
            .Create<EmailSenderJob>()
            .WithIdentity($"emailSenderJob_{Guid.NewGuid()}", "emails")
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .StartAt(sendGridAccessibilityService.GetAccessibilityTime(DateTimeOffset.Now))
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }

    private string EncodeToBase64(string toEncode)
    {
        byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);

        string returnValue = Convert.ToBase64String(toEncodeAsBytes);

        return returnValue;
    }
}