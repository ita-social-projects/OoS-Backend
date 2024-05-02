using System;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.EmailSender.Quartz;
using Quartz;

namespace OutOfSchool.EmailSender.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly ISchedulerFactory schedulerFactory;

    public EmailSenderService(ISchedulerFactory schedulerFactory)
    {
        this.schedulerFactory = schedulerFactory;
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
            { EmailSenderStringConstants.ExpirationTime, expirationTime.ToString() },
        };

        var job = JobBuilder.Create<EmailSenderJob>()
            .UsingJobData(jobData)
            .StoreDurably()
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();
        await scheduler.AddJob(job, false);
    }

    private string EncodeToBase64(string toEncode)
    {
        byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);

        string returnValue = Convert.ToBase64String(toEncodeAsBytes);

        return returnValue;
    }
}